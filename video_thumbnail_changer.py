import tkinter as tk
from tkinter import filedialog, messagebox, colorchooser, scrolledtext
from tkinter import ttk
import subprocess
import os
from PIL import Image, ImageDraw, ImageFont, ImageTk
import threading
from pathlib import Path
import json
import shutil
from datetime import datetime
import ctypes
import sys
import time

try:
    import vlc
    VLC_AVAILABLE = True
except ImportError:
    VLC_AVAILABLE = False

class VideoThumbnailChanger:
    def __init__(self, root):
        self.root = root
        self.root.title("Video Thumbnail Changer")
        self.root.geometry("1200x700")
        self.root.resizable(False, False)
        
        # ===== LAYOUT DIMENSIONS (easy to configure) =====
        # Format: X;Y;WIDTH;HEIGHT for each zone
        self.GALLERY_ZONE = "0;25;190;675"          # X;Y;Width;Height
        self.FRAMES_ZONE = "200;25;197;675"         # X;Y;Width;Height
        self.PLAYER_ZONE = "402;25;523;533"         # X;Y;Width;Height
        self.TEXT_ZONE = "980;25;230;170"           # X;Y;Width;Height
        self.SCREEN_ZONE = "980;195;230;210"        # X;Y;Width;Height
        self.THUMBNAIL_ZONE = "980;433;230;110"     # X;Y;Width;Height
        # ===== END LAYOUT DIMENSIONS =====
        
        # Fixed image frame dimensions for montage
        self.FRAME_WIDTH = 640
        self.FRAME_HEIGHT = 360
        self.MONTAGE_WIDTH = 1280
        self.MONTAGE_HEIGHT = 720
        
        # Configuration
        self._pending_config = None
        self.current_folder = None
        self.video_path = None
        self.video_frames = {}
        self.current_frame_percent = 25
        self.current_image_pil = None
        self.overlay_text = tk.StringVar(value="JPG")
        self.text_color = (255, 255, 0)
        self.text_bg_color = (0, 0, 0)
        self.text_size = 30
        self.text_position = "down-center"
        self.processing = False
        # Config file - use pathlib for reliability
        self.config_file = Path(__file__).parent / "thumbnail_config.json"
        self.temp_dir = None
        self.video_duration = 0
        self.thumb_width = 150
        self.is_playing = False
        self.screenshots = {}
        self.screenshots_dir = None
        self.vlc_seeking = False
        self.final_thumbnail_pil = None
        self.video_info = {}
        self.has_existing_thumbnail = False
        
        # UI variables
        self.progress_var = tk.IntVar(value=0)
        self.width_var = tk.IntVar(value=1280)
        self.height_var = tk.IntVar(value=720)
        self.position_var = tk.StringVar(value="down-center")
        self.percentages_var = tk.StringVar(value="0,10,20,30,40,50,60,70,80,90")
        
        # VLC
        self.vlc_instance = None
        self.vlc_player = None
        self.vlc_media = None
        self.vlc_lib_path = None
        
        # Load config
        self.load_config()
        
        # Check FFmpeg
        self.check_ffmpeg()
        
        # Check VLC
        self.check_vlc()
        
        # Check Icaros
        self.icaros_available = self.check_icaros()
        
        # Setup UI
        self.setup_ui()
        
        # Load previous data
        self.load_previous_data()
        
        # Load Icaros path if available
        self.load_icaros_path()
        
        # Force layout dimensions after UI is created
        self.root.after(300, self.force_layout_dimensions)
        self.log("[INFO] Application started")
    
    def check_thumbnail_support(self):
        """Check if current video format supports embedded thumbnails"""
        if not self.video_path:
            # No video selected - disable buttons
            if hasattr(self, 'apply_btn'):
                self.apply_btn.config(state=tk.DISABLED)
            if hasattr(self, 'reencode_btn'):
                self.reencode_btn.config(state=tk.DISABLED)
            if hasattr(self, 'mkv_apply_btn'):
                self.mkv_apply_btn.config(state=tk.DISABLED)
            return False
        
        # Formats that support embedded thumbnails (tested and working)
        # Removed .mov - Icaros doesn't support it well
        supported_formats = ['.mp4', '.m4v', '.mkv', '.avi', '.webm']
        ext = os.path.splitext(self.video_path)[1].lower()
        
        supports = ext in supported_formats
        has_thumbnail = self.final_thumbnail_pil is not None
        is_mkv = ext == '.mkv'
        
        # Apply button: only for supported formats
        apply_state = tk.NORMAL if (supports and has_thumbnail) else tk.DISABLED
        
        if hasattr(self, 'apply_btn'):
            self.apply_btn.config(state=apply_state)
        
        # Reencode button: always enabled when thumbnail exists (allows format conversion)
        reencode_state = tk.NORMAL if has_thumbnail else tk.DISABLED
        if hasattr(self, 'reencode_btn'):
            self.reencode_btn.config(state=reencode_state)
        
        # MKV Apply button only for MKV files
        if hasattr(self, 'mkv_apply_btn'):
            mkv_state = tk.NORMAL if (is_mkv and has_thumbnail) else tk.DISABLED
            self.mkv_apply_btn.config(state=mkv_state)
        
        if not supports:
            self.log(f"[INFO] Format {ext} does not support thumbnails (supported: {', '.join(supported_formats)})")
            self.log(f"[INFO] Use 'Reencode' to convert to a supported format")
        
        return supports
    
    def auto_generate_thumbnail(self):
        """Auto-generate thumbnail from frame at 25% if empty"""
        if self.final_thumbnail_pil is not None:
            return  # Already has a thumbnail
        
        # Use frame at 25% as default thumbnail
        if 25 in self.video_frames:
            self.select_frame_percent(25)
            self.log("[OK] Default thumbnail set to 25% frame")
        else:
            # Fallback: update preview which will create thumbnail from black image
            self.update_preview_live()
        
        # Enable buttons
        self.check_thumbnail_support()
    
    def load_previous_data(self):
        """Load previous thumbnails, screenshots, and frames if available"""
        # DISABLED: Don't auto-load previous data at startup
        # This prevents cluttering the UI with old data
        # Data will be loaded fresh when a video is selected
        pass
    
    def download_icaros(self):
        """Opens Icaros releases page in browser"""
        import webbrowser
        url = "https://github.com/Xanashi/Icaros/releases"
        self.log(f"[INFO] Opening Icaros releases page...")
        webbrowser.open(url)
        messagebox.showinfo("Download Icaros", 
            f"Opening GitHub releases page...\n\n"
            f"After downloading and installing Icaros:\n"
            f"1. Run the installer\n"
            f"2. Click 'Browse carosConfig.exe' below to select the path\n"
            f"3. The path will be saved in your config\n\n"
            f"URL: {url}")
    
    def select_icaros_path(self):
        """Select carosConfig.exe path"""
        path = filedialog.askopenfilename(
            title="Select carosConfig.exe",
            filetypes=[("Icaros Config", "carosConfig.exe"), ("All files", "*.*")]
        )
        
        if path and os.path.exists(path):
            self.icaros_path_var.set(path)
            self.log(f"[OK] Icaros path set to: {path}")
            self.icaros_status_label.config(text="✓ Installed", foreground="green")
            messagebox.showinfo("Success", f"Icaros path saved:\n{path}")
            
            # Save to config
            self.save_config()
        else:
            messagebox.showerror("Error", "carosConfig.exe not found")
    
    def load_icaros_path(self):
        """Load Icaros path from config"""
        # Variables created in setup_ui, safe to access now
        if hasattr(self, '_pending_config') and self._pending_config and 'icaros_path' in self._pending_config:
            path = self._pending_config['icaros_path']
            if path and os.path.exists(path):
                self.icaros_path_var.set(path)
                self.icaros_status_label.config(text="✓ Installed", foreground="green")
                self.log(f"[OK] Icaros found: {path}")
                return path
        
        self.icaros_status_label.config(text="Not installed", foreground="red")
        return None
    
    def check_icaros(self):
        """Check if Icaros is installed"""
        # First check saved path
        if hasattr(self, '_pending_config') and self._pending_config and 'icaros_path' in self._pending_config:
            path = self._pending_config['icaros_path']
            if path and os.path.exists(path):
                self.log("[OK] Icaros found (from config)")
                return True
        
        # Then check standard paths
        icaros_paths = [
            r"C:\Program Files\Icaros",
            r"C:\Program Files (x86)\Icaros",
        ]
        
        for path in icaros_paths:
            if os.path.exists(path):
                self.log("[OK] Icaros found in standard path")
                return True
        
        self.log("[WARNING] Icaros not found - install from https://github.com/Xanashi/Icaros")
        return False
    
    def apply_mkv_windows(self):
        """Apply thumbnail to MKV using Windows native properties"""
        if not self.is_mkv_format():
            messagebox.showerror("Error", "This option is only for MKV files")
            return
        
        if not self.final_thumbnail_pil:
            messagebox.showerror("Error", "Create a thumbnail first")
            return
        
        self.log("[INFO] ===== APPLYING THUMBNAIL (MKV WINDOWS) =====")
        
        script_dir = os.path.dirname(os.path.abspath(__file__))
        temp_thumb = os.path.join(script_dir, "temp_thumb.jpg")
        
        def apply_mkv():
            try:
                self.log("[INFO] Creating final image...")
                
                final_image = self.final_thumbnail_pil.copy()
                output_width = self.width_var.get()
                output_height = self.height_var.get()
                final_image = final_image.resize((output_width, output_height), Image.Resampling.LANCZOS)
                self.log(f"[INFO] Resized to {output_width}x{output_height}")
                
                final_image.save(temp_thumb, quality=95)
                self.log("[OK] Image saved")
                self.progress_var.set(30)
                
                # Use FFmpeg to embed in MKV
                self.log("[INFO] Embedding thumbnail in MKV...")
                
                output_path = os.path.splitext(self.video_path)[0] + "_temp.mkv"
                cmd = [
                    'ffmpeg',
                    '-i', self.video_path,
                    '-i', temp_thumb,
                    '-map', '0',
                    '-map', '1',
                    '-c', 'copy',
                    '-c:a', 'copy',
                    '-disposition:v:1', 'attached_pic',
                    '-y',
                    output_path
                ]
                
                result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace')
                self.progress_var.set(70)
                
                if result.returncode != 0:
                    raise Exception(result.stderr[:200])
                
                os.remove(temp_thumb)
                
                # Replace original with new file
                backup_path = self.video_path + ".backup"
                if not os.path.exists(backup_path):
                    shutil.copy(self.video_path, backup_path)
                    self.log("[OK] Backup created")
                
                shutil.move(output_path, self.video_path)
                
                self.progress_var.set(100)
                self.log("[SUCCESS] MKV thumbnail applied!")
                
                # Auto-refresh cache
                self.log("[INFO] Auto-refreshing Windows cache...")
                self.root.after(1000, self.refresh_windows_thumbnails_silent)
                
                messagebox.showinfo("Success", "MKV thumbnail applied!\n\nCache is being refreshed...\nYour thumbnail will appear in Windows Explorer shortly.")
                
            except Exception as e:
                self.log(f"[ERROR] {str(e)}")
                messagebox.showerror("Error", f"Error : {str(e)}")
                if os.path.exists(temp_thumb):
                    os.remove(temp_thumb)
                self.progress_var.set(0)
        
        thread = threading.Thread(target=apply_mkv, daemon=True)
        thread.start()
    
    def refresh_windows_thumbnails_silent(self):
        """Refresh Windows cache silently without messagebox"""
        try:
            self.log("[INFO] Clearing Windows thumbnail cache...")
            
            # Kill Explorer to release lock on cache
            os.system("taskkill /F /IM explorer.exe >nul 2>&1")
            time.sleep(1)
            
            # Clear Windows thumbnail cache
            cache_path = os.path.expandvars(r"%LocalAppData%\Microsoft\Windows\Explorer")
            if os.path.exists(cache_path):
                deleted = 0
                for file in os.listdir(cache_path):
                    if file.startswith("thumbcache") and file.endswith(".db"):
                        try:
                            os.remove(os.path.join(cache_path, file))
                            deleted += 1
                        except:
                            pass
                
                self.log(f"[OK] Cache cleared ({deleted} file(s))")
            
            # Restart Explorer
            self.log("[INFO] Restarting Windows Explorer...")
            os.startfile("explorer.exe")
            time.sleep(2)
            
            self.log("[SUCCESS] Cache refreshed - thumbnails ready!")
                
        except Exception as e:
            self.log(f"[WARNING] Could not auto-refresh cache: {str(e)}")
    
    def refresh_windows_thumbnails(self):
        """Refresh Windows thumbnail cache and Explorer"""
        try:
            self.log("[INFO] Clearing Windows thumbnail cache...")
            
            # Kill Explorer to release lock on cache
            os.system("taskkill /F /IM explorer.exe >nul 2>&1")
            time.sleep(1)
            
            # Clear Windows thumbnail cache
            cache_path = os.path.expandvars(r"%LocalAppData%\Microsoft\Windows\Explorer")
            if os.path.exists(cache_path):
                deleted = 0
                for file in os.listdir(cache_path):
                    if file.startswith("thumbcache") and file.endswith(".db"):
                        try:
                            os.remove(os.path.join(cache_path, file))
                            deleted += 1
                        except:
                            pass
                
                self.log(f"[OK] Deleted {deleted} cache file(s)")
            
            # Restart Explorer
            self.log("[INFO] Restarting Windows Explorer...")
            os.startfile("explorer.exe")
            time.sleep(2)
            
            self.log("[SUCCESS] Cache cleared and Explorer restarted!")
            messagebox.showinfo("Success", 
                "✓ Windows thumbnail cache cleared\n"
                "✓ Windows Explorer restarted\n\n"
                "Your new thumbnails will now appear in Explorer!")
                
        except Exception as e:
            self.log(f"[ERROR] {str(e)}")
            messagebox.showerror("Error", f"Error: {str(e)}\n\nTry restarting Windows Explorer manually")
    
    def is_mkv_format(self):
        """Check if current video is MKV format"""
        if not self.video_path:
            return False
        return os.path.splitext(self.video_path)[1].lower() == '.mkv'
    
    def parse_zone_dims(self, zone_string):
        """Parse X;Y;WIDTH;HEIGHT format and return dict"""
        parts = zone_string.split(';')
        if len(parts) == 4:
            return {
                'x': int(parts[0]),
                'y': int(parts[1]),
                'width': int(parts[2]),
                'height': int(parts[3])
            }
        return {'x': 0, 'y': 0, 'width': 100, 'height': 100}
    
    def log(self, message):
        """Adds a message to the log"""
        timestamp = datetime.now().strftime("%H:%M:%S")
        log_msg = f"[{timestamp}] {message}\n"
        
        if hasattr(self, 'log_text'):
            self.log_text.config(state=tk.NORMAL)
            self.log_text.insert(tk.END, log_msg)
            self.log_text.see(tk.END)
            self.log_text.config(state=tk.DISABLED)
            self.root.update()
    
    def log_layout(self, label="LAYOUT"):
        """Log all layout dimensions in X;Y;WIDTH;HEIGHT format"""
        # Disabled - no dimension logging
        pass
    
    def force_layout_dimensions(self):
        """Force gallery and frames to fixed widths"""
        try:
            # Parse all zone dimensions
            gallery_dims = self.parse_zone_dims(self.GALLERY_ZONE)
            frames_dims = self.parse_zone_dims(self.FRAMES_ZONE)
            text_dims = self.parse_zone_dims(self.TEXT_ZONE)
            
            # Get actual parent dimensions
            parent_width = self.root.winfo_width()
            parent_height = self.root.winfo_height()
            
            # Gallery: use parsed dimensions
            if hasattr(self, 'gallery_frame'):
                self.gallery_frame.configure(width=gallery_dims['width'], height=parent_height)
            
            # Frames: use parsed dimensions
            if hasattr(self, 'frames_frame'):
                self.frames_frame.configure(width=frames_dims['width'], height=parent_height)
            
            # Right options: use parsed dimensions from TEXT_ZONE (width)
            if hasattr(self, 'right_paned'):
                self.right_paned.configure(width=text_dims['width'], height=parent_height)
        except Exception as e:
            pass
    
    def check_vlc(self):
        """Checks and initializes VLC"""
        if not VLC_AVAILABLE:
            self.log("[WARNING] python-vlc not available")
            return False
        
        vlc_paths = [
            r"C:\Program Files\VideoLAN\VLC",
            r"C:\Program Files (x86)\VideoLAN\VLC",
            "/usr/bin",
            "/usr/local/bin"
        ]
        
        found = False
        for path in vlc_paths:
            if os.path.exists(path):
                os.environ['VLC_PLUGIN_PATH'] = path
                self.vlc_lib_path = path
                found = True
                break
        
        if not found:
            path = filedialog.askdirectory(title="Select VLC folder", initialdir=r"C:\Program Files\VideoLAN\VLC")
            if path:
                os.environ['VLC_PLUGIN_PATH'] = path
                self.vlc_lib_path = path
                found = True
        
        if found:
            try:
                self.vlc_instance = vlc.Instance()
                self.vlc_player = self.vlc_instance.media_list_player_new()
                self.log("[OK] VLC initialized")
                return True
            except Exception as e:
                self.log(f"[ERROR] VLC init : {str(e)}")
                return False
        else:
            self.log("[ERROR] VLC not found")
            return False
    
    def setup_ui(self):
        """Sets up the interface with logs always visible"""
        # Main container (vertical: notebook on top, logs at bottom)
        main_window = ttk.Frame(self.root)
        main_window.pack(fill=tk.BOTH, expand=True, padx=0, pady=0)
        main_window.columnconfigure(0, weight=1)
        main_window.rowconfigure(0, weight=1)  # Notebook gets extra space
        main_window.rowconfigure(1, weight=0)  # Logs stay fixed height
        
        # Notebook (takes most of the space)
        notebook = ttk.Notebook(main_window)
        notebook.grid(row=0, column=0, sticky="nsew", padx=0, pady=0)
        
        app_frame = ttk.Frame(notebook)
        notebook.add(app_frame, text="📹 Application")
        self.setup_app_tab(app_frame)
        
        software_frame = ttk.Frame(notebook)
        notebook.add(software_frame, text="🛠️ Software")
        self.setup_software_tab(software_frame)
        
        config_frame = ttk.Frame(notebook)
        notebook.add(config_frame, text="⚙️ Settings")
        self.setup_config_tab(config_frame)
        
        # ===== LOGS WINDOW (Always Visible) =====
        log_frame = ttk.LabelFrame(main_window, text="📋 Logs", padding="3")
        log_frame.grid(row=1, column=0, sticky="ew", padx=3, pady=3)
        log_frame.rowconfigure(0, weight=1)
        log_frame.columnconfigure(0, weight=1)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=4, state=tk.DISABLED, 
                                                  bg="black", fg="#00FF00", font=("Courier", 7))
        self.log_text.pack(fill=tk.BOTH, expand=True)
    
    def setup_app_tab(self, parent):
        """Main interface"""
        # Main vertical layout
        main_container = ttk.Frame(parent)
        main_container.pack(fill=tk.BOTH, expand=True, padx=0, pady=0)
        main_container.columnconfigure(0, weight=1)
        main_container.rowconfigure(0, weight=1)
        main_container.rowconfigure(1, weight=0)
        
        # Horizontal paned window (4 columns) - FIXED LAYOUT
        main_paned = ttk.PanedWindow(main_container, orient=tk.HORIZONTAL)
        main_paned.grid(row=0, column=0, sticky="nsew")
        
        # Log widget dimensions (removed - panes are now fixed)
        # main_paned.bind("<Configure>", lambda e: self.log_pane_dimensions(e, "MainPane"))
        
        # ===== 1. GALLERY (LEFT) =====
        gallery_dims = self.parse_zone_dims(self.GALLERY_ZONE)
        self.gallery_frame = ttk.LabelFrame(main_paned, text="📁 Gallery", padding="5")
        self.gallery_frame.pack_propagate(False)
        self.gallery_frame.configure(width=gallery_dims['width'], height=gallery_dims['height'])
        main_paned.add(self.gallery_frame, weight=0)
        self.gallery_frame.columnconfigure(0, weight=1)
        self.gallery_frame.rowconfigure(2, weight=1)
        
        ttk.Button(self.gallery_frame, text="Open Folder", command=self.select_folder).pack(fill=tk.X, pady=5)
        self.folder_label = ttk.Label(self.gallery_frame, text="No folder", foreground="gray", wraplength=140)
        self.folder_label.pack(fill=tk.X, pady=3)
        
        canvas_frame = tk.Frame(self.gallery_frame)
        canvas_frame.pack(fill=tk.BOTH, expand=True, pady=5)
        
        canvas = tk.Canvas(canvas_frame, bg="gray20", highlightthickness=0, width=150)
        scrollbar = ttk.Scrollbar(canvas_frame, orient=tk.VERTICAL, command=canvas.yview)
        
        self.gallery_container = tk.Frame(canvas, bg="gray20")
        def on_gallery_configure(e):
            canvas.configure(scrollregion=canvas.bbox("all"))
        self.gallery_container.bind("<Configure>", on_gallery_configure)
        
        canvas.create_window((0, 0), window=self.gallery_container, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        
        # Add mousewheel scroll support (bind to canvas only)
        def on_gallery_mousewheel(event):
            canvas.yview_scroll(int(-1*(event.delta/120)), "units")
        canvas.bind("<MouseWheel>", on_gallery_mousewheel)
        canvas.bind("<Button-4>", lambda e: canvas.yview_scroll(-1, "units"))
        canvas.bind("<Button-5>", lambda e: canvas.yview_scroll(1, "units"))
        canvas.bind("<Enter>", lambda e: canvas.focus_set())  # Focus on hover
        
        canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.gallery_thumbnails = []
        
        # ===== 2. FRAMES GRID 2x5 =====
        frames_dims = self.parse_zone_dims(self.FRAMES_ZONE)
        self.frames_frame = ttk.LabelFrame(main_paned, text="🎬 Frames", padding="5")
        self.frames_frame.pack_propagate(False)
        self.frames_frame.configure(width=frames_dims['width'], height=frames_dims['height'])
        main_paned.add(self.frames_frame, weight=0)
        self.frames_frame.columnconfigure(0, weight=1)
        self.frames_frame.columnconfigure(1, weight=1)
        self.frames_frame.rowconfigure(0, weight=1)
        
        self.frames_container = tk.Frame(self.frames_frame, bg="gray20")
        self.frames_container.pack(fill=tk.BOTH, expand=True)
        self.frames_container.columnconfigure(0, weight=1)
        self.frames_container.columnconfigure(1, weight=1)
        
        self.frame_images = {}
        for row in range(5):
            self.frames_container.rowconfigure(row, weight=1)
            for col in range(2):
                frame_widget = tk.Frame(self.frames_container, bg="gray40", relief=tk.SUNKEN, bd=2)
                frame_widget.grid(row=row, column=col, sticky="nsew", padx=2, pady=2)
                frame_widget.rowconfigure(0, weight=1)
                frame_widget.columnconfigure(0, weight=1)
                
                frame_num = row * 2 + col
                percent = frame_num * 10
                
                img_label = tk.Label(frame_widget, bg="black", cursor="hand2", text=f"{percent}%", fg="white")
                img_label.grid(row=0, column=0, sticky="nsew")
                
                def make_click_handler(p):
                    def on_click(e):
                        self.select_frame_percent(p)
                    return on_click
                
                img_label.bind("<Button-1>", make_click_handler(percent))
                img_label.bind("<Enter>", lambda e, w=frame_widget: w.config(bg="gray50"))
                img_label.bind("<Leave>", lambda e, w=frame_widget: w.config(bg="gray40"))
                
                self.frame_images[percent] = img_label
        
        # ===== 3. CENTER PANEL (PLAYER) =====
        center_panel = ttk.Frame(main_paned)
        main_paned.add(center_panel, weight=1)
        center_panel.columnconfigure(0, weight=1)
        center_panel.rowconfigure(0, weight=0)
        center_panel.rowconfigure(1, weight=1)
        center_panel.rowconfigure(2, weight=0)
        
        # Video Info Panel
        info_frame = ttk.LabelFrame(center_panel, text="📊 Video Info", padding="3")
        info_frame.pack(fill=tk.X, padx=3, pady=3)
        info_frame.columnconfigure(1, weight=1)
        info_frame.columnconfigure(3, weight=1)
        info_frame.columnconfigure(5, weight=1)
        info_frame.columnconfigure(7, weight=1)
        
        ttk.Label(info_frame, text="Size:", font=("", 8)).grid(row=0, column=0, sticky="w")
        self.info_size = ttk.Label(info_frame, text="—", font=("", 8), foreground="blue")
        self.info_size.grid(row=0, column=1, sticky="w", padx=5)
        
        ttk.Label(info_frame, text="Duration:", font=("", 8)).grid(row=0, column=2, sticky="w")
        self.info_duration = ttk.Label(info_frame, text="—", font=("", 8), foreground="blue")
        self.info_duration.grid(row=0, column=3, sticky="w", padx=5)
        
        ttk.Label(info_frame, text="Format:", font=("", 8)).grid(row=0, column=4, sticky="w")
        self.info_format = ttk.Label(info_frame, text="—", font=("", 8), foreground="blue")
        self.info_format.grid(row=0, column=5, sticky="w", padx=5)
        
        ttk.Label(info_frame, text="Thumbnail:", font=("", 8)).grid(row=0, column=6, sticky="w")
        self.info_thumbnail = ttk.Label(info_frame, text="No", font=("", 8), foreground="red")
        self.info_thumbnail.grid(row=0, column=7, sticky="w", padx=5)
        
        # Video name
        self.video_name_label = ttk.Label(center_panel, text="No video", font=("", 9, "bold"), foreground="blue")
        self.video_name_label.pack(fill=tk.X, padx=5, pady=3)
        
        # Player
        player_frame = ttk.LabelFrame(center_panel, text="👁️ VLC Player", padding="3")
        player_frame.pack(fill=tk.BOTH, expand=True, padx=3, pady=3)
        player_frame.columnconfigure(0, weight=1)
        player_frame.rowconfigure(0, weight=1)
        
        self.vlc_canvas = tk.Canvas(player_frame, bg="black", width=300, height=250)
        self.vlc_canvas.pack(fill=tk.BOTH, expand=True)
        
        # Add keyboard bindings for seeking
        self.root.bind("<Left>", lambda e: self.seek_video(-1))
        self.root.bind("<Right>", lambda e: self.seek_video(1))
        self.root.bind("<Up>", lambda e: self.seek_video(5))
        self.root.bind("<Down>", lambda e: self.seek_video(-5))
        
        # Timeline
        timeline_frame = ttk.Frame(center_panel)
        timeline_frame.pack(fill=tk.X, padx=3, pady=2)
        timeline_frame.columnconfigure(1, weight=1)
        
        ttk.Label(timeline_frame, text="00:00", width=6, font=("", 8)).pack(side=tk.LEFT)
        self.timeline_var = tk.DoubleVar(value=0)
        self.timeline_scale = ttk.Scale(timeline_frame, from_=0, to=100, variable=self.timeline_var, 
                                       orient=tk.HORIZONTAL, command=self.on_timeline_change)
        self.timeline_scale.pack(side=tk.LEFT, padx=3, fill=tk.X, expand=True)
        
        self.time_label = ttk.Label(timeline_frame, text="00:00 / 00:00", width=12, font=("", 8))
        self.time_label.pack(side=tk.LEFT, padx=3)
        
        # Controls
        controls_frame = ttk.Frame(center_panel)
        controls_frame.pack(fill=tk.X, padx=3, pady=2)
        
        self.play_button = ttk.Button(controls_frame, text="▶️", command=self.play_video, width=3)
        self.play_button.pack(side=tk.LEFT, padx=2)
        ttk.Button(controls_frame, text="⏸️", command=self.pause_video, width=3).pack(side=tk.LEFT, padx=2)
        ttk.Button(controls_frame, text="⏹️", command=self.stop_video, width=3).pack(side=tk.LEFT, padx=2)
        ttk.Button(controls_frame, text="📸", command=self.save_screenshot, width=3).pack(side=tk.LEFT, padx=2)
        
        ttk.Label(controls_frame, text="Vol:", font=("", 8)).pack(side=tk.LEFT, padx=5)
        self.volume_var = tk.IntVar(value=100)
        ttk.Scale(controls_frame, from_=0, to=100, variable=self.volume_var, 
                 orient=tk.HORIZONTAL, length=80, command=self.on_volume_change).pack(side=tk.LEFT, padx=2)
        
        # ===== 4. RIGHT PANEL (OPTIONS) =====
        text_dims = self.parse_zone_dims(self.TEXT_ZONE)
        self.right_paned = ttk.PanedWindow(main_paned, orient=tk.VERTICAL)
        self.right_paned.pack_propagate(False)
        self.right_paned.configure(width=text_dims['width'], height=700)
        main_paned.add(self.right_paned, weight=0)
        # Logging removed - right pane is now fixed
        
        # 4a. TEXT OPTIONS (small, fixed ~170px)
        self.text_frame = ttk.LabelFrame(self.right_paned, text="✨ Text", padding="4")
        self.text_frame.pack_propagate(False)
        self.text_frame.configure(height=text_dims['height'])
        self.right_paned.add(self.text_frame, weight=0)
        self.text_frame.columnconfigure(1, weight=1)
        
        ttk.Label(self.text_frame, text="Text:", font=("", 8)).grid(row=0, column=0, sticky="w", padx=3)
        text_entry = ttk.Entry(self.text_frame, textvariable=self.overlay_text, width=15, font=("", 8))
        text_entry.grid(row=0, column=1, sticky="ew", padx=3)
        text_entry.bind("<KeyRelease>", lambda e: self.update_preview_live())
        
        ttk.Label(self.text_frame, text="Size:", font=("", 8)).grid(row=1, column=0, sticky="w", padx=3)
        self.size_spinbox = ttk.Spinbox(self.text_frame, from_=10, to=100, width=8, font=("", 8))
        self.size_spinbox.set(30)
        self.size_spinbox.grid(row=1, column=1, sticky="w", padx=3)
        self.size_spinbox.bind("<KeyRelease>", lambda e: self.update_preview_live())
        
        # Colors side by side
        colors_frame = tk.Frame(self.text_frame)
        colors_frame.grid(row=2, column=0, columnspan=2, sticky="ew", padx=3, pady=2)
        colors_frame.columnconfigure(0, weight=1)
        colors_frame.columnconfigure(1, weight=1)
        
        ttk.Button(colors_frame, text="Text Color", command=self.choose_text_color).pack(side=tk.LEFT, padx=2, fill=tk.X, expand=True)
        ttk.Button(colors_frame, text="BG Color", command=self.choose_text_bg_color).pack(side=tk.LEFT, padx=2, fill=tk.X, expand=True)
        
        # Color samples side by side
        samples_frame = tk.Frame(self.text_frame)
        samples_frame.grid(row=3, column=0, columnspan=2, sticky="ew", padx=3, pady=2)
        samples_frame.columnconfigure(0, weight=1)
        samples_frame.columnconfigure(1, weight=1)
        
        self.color_label = tk.Label(samples_frame, bg="#FFFF00", height=1)
        self.color_label.pack(side=tk.LEFT, padx=2, fill=tk.BOTH, expand=True)
        
        self.color_bg_label = tk.Label(samples_frame, bg="#000000", height=1)
        self.color_bg_label.pack(side=tk.LEFT, padx=2, fill=tk.BOTH, expand=True)
        
        ttk.Label(self.text_frame, text="Pos:", font=("", 8)).grid(row=4, column=0, sticky="w", padx=3)
        pos_combo = ttk.Combobox(self.text_frame, textvariable=self.position_var,
                    values=["top-left", "top-center", "top-right", "down-left", "down-center", "down-right", "center"],
                    state="readonly", width=13, font=("", 8))
        pos_combo.grid(row=4, column=1, sticky="ew", padx=3)
        pos_combo.bind("<<ComboboxSelected>>", lambda e: self.update_preview_live())
        
        # 4b. SCREENSHOTS (same height as Final ~165px)
        screen_dims = self.parse_zone_dims(self.SCREEN_ZONE)
        self.screens_frame = ttk.LabelFrame(self.right_paned, text="📸 Screenshots", padding="4")
        self.screens_frame.pack_propagate(False)
        self.screens_frame.configure(height=screen_dims['height'])
        self.right_paned.add(self.screens_frame, weight=0)
        self.screens_frame.columnconfigure(0, weight=1)
        self.screens_frame.rowconfigure(0, weight=1)
        
        screens_canvas_frame = tk.Frame(self.screens_frame)
        screens_canvas_frame.pack(fill=tk.BOTH, expand=True)
        screens_canvas_frame.rowconfigure(0, weight=1)
        screens_canvas_frame.columnconfigure(0, weight=1)
        
        screens_canvas = tk.Canvas(screens_canvas_frame, bg="gray20", highlightthickness=0)
        screens_scrollbar = ttk.Scrollbar(screens_canvas_frame, orient=tk.VERTICAL, command=screens_canvas.yview)
        
        self.screenshots_container = tk.Frame(screens_canvas, bg="gray20")
        def on_screenshots_configure(e):
            screens_canvas.configure(scrollregion=screens_canvas.bbox("all"))
        self.screenshots_container.bind("<Configure>", on_screenshots_configure)
        
        screens_canvas.create_window((0, 0), window=self.screenshots_container, anchor="nw")
        screens_canvas.configure(yscrollcommand=screens_scrollbar.set)
        
        # Add mousewheel scroll support (bind to canvas only)
        def on_screenshots_mousewheel(event):
            screens_canvas.yview_scroll(int(-1*(event.delta/120)), "units")
        screens_canvas.bind("<MouseWheel>", on_screenshots_mousewheel)
        screens_canvas.bind("<Button-4>", lambda e: screens_canvas.yview_scroll(-1, "units"))
        screens_canvas.bind("<Button-5>", lambda e: screens_canvas.yview_scroll(1, "units"))
        screens_canvas.bind("<Enter>", lambda e: screens_canvas.focus_set())  # Focus on hover
        
        screens_canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        screens_scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.screenshots_widgets = {}
        
        # 4c. FINAL THUMBNAIL + BUTTONS
        thumb_dims = self.parse_zone_dims(self.THUMBNAIL_ZONE)
        self.thumb_frame = ttk.LabelFrame(self.right_paned, text="🎨 Final", padding="4")
        self.thumb_frame.pack_propagate(False)
        self.thumb_frame.configure(height=thumb_dims['height'])
        self.right_paned.add(self.thumb_frame, weight=0)
        self.thumb_frame.columnconfigure(0, weight=1)
        self.thumb_frame.rowconfigure(0, weight=1)
        
        self.final_thumb_label = tk.Label(self.thumb_frame, bg="black", width=150, height=70)
        self.final_thumb_label.pack(fill=tk.BOTH, expand=True, pady=3)
        
        # Buttons - Row 1: Apply and Reencode
        buttons_frame = tk.Frame(self.thumb_frame)
        buttons_frame.pack(fill=tk.X, padx=3, pady=3)
        buttons_frame.columnconfigure(0, weight=1)
        buttons_frame.columnconfigure(1, weight=1)
        buttons_frame.columnconfigure(2, weight=1)
        buttons_frame.columnconfigure(3, weight=1)
        
        self.apply_btn = ttk.Button(buttons_frame, text="✅ Apply", command=self.apply_thumbnail, state=tk.DISABLED)
        self.apply_btn.grid(row=0, column=0, sticky="ew", padx=2)
        
        self.reencode_btn = ttk.Button(buttons_frame, text="🔄 Reencode", command=self.reencode_with_thumbnail, state=tk.DISABLED)
        self.reencode_btn.grid(row=0, column=1, sticky="ew", padx=2)
        
        # MKV Windows Apply button (only shows for MKV files)
        self.mkv_apply_btn = ttk.Button(buttons_frame, text="🪟 MKV Apply", command=self.apply_mkv_windows, state=tk.DISABLED)
        self.mkv_apply_btn.grid(row=0, column=2, sticky="ew", padx=2)
        
        # Icaros Refresh button
        self.icaros_btn = ttk.Button(buttons_frame, text="🔄 Refresh Cache", command=self.refresh_windows_thumbnails, state=tk.NORMAL)
        self.icaros_btn.grid(row=0, column=3, sticky="ew", padx=2)
        
        # Progress bar
        self.progress_var = tk.IntVar(value=0)
        self.progress_bar = ttk.Progressbar(self.thumb_frame, variable=self.progress_var, maximum=100)
        self.progress_bar.pack(fill=tk.X, padx=3, pady=2)
        
        # Logs are now in the main window, always visible (created in setup_ui)
    
    def setup_software_tab(self, parent):
        """Software configuration tab with FFmpeg, VLC, and Icaros"""
        # Create a canvas with scrollbar for scrollable content
        canvas = tk.Canvas(parent, bg="white", highlightthickness=0)
        scrollbar = ttk.Scrollbar(parent, orient=tk.VERTICAL, command=canvas.yview)
        
        software_frame = ttk.Frame(canvas, padding="10")
        software_frame.pack(fill=tk.BOTH, expand=True)
        
        # Bind the scroll event
        def on_mousewheel(event):
            canvas.yview_scroll(int(-1*(event.delta/120)), "units")
        canvas.bind_all("<MouseWheel>", on_mousewheel)
        
        canvas.create_window((0, 0), window=software_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        
        # Update scroll region when frame changes
        def on_frame_configure(event=None):
            canvas.configure(scrollregion=canvas.bbox("all"))
        software_frame.bind("<Configure>", on_frame_configure)
        
        canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # ===== FFmpeg Section =====
        ffmpeg_section = ttk.LabelFrame(software_frame, text="FFmpeg", padding="10")
        ffmpeg_section.pack(fill=tk.X, pady=10)
        ffmpeg_section.columnconfigure(1, weight=1)
        
        ttk.Label(ffmpeg_section, text="Path:").grid(row=0, column=0, sticky="w")
        self.ffmpeg_path_var = tk.StringVar()
        ttk.Entry(ffmpeg_section, textvariable=self.ffmpeg_path_var, width=50).grid(row=0, column=1, sticky="ew", padx=5)
        ttk.Button(ffmpeg_section, text="Browse...", command=self.select_ffmpeg_path).grid(row=0, column=2)
        
        ttk.Button(ffmpeg_section, text="Test FFmpeg", command=self.test_ffmpeg).grid(row=1, column=0, columnspan=3, pady=10)
        
        # ===== VLC Section =====
        vlc_section = ttk.LabelFrame(software_frame, text="VLC", padding="10")
        vlc_section.pack(fill=tk.X, pady=10)
        vlc_section.columnconfigure(1, weight=1)
        
        ttk.Label(vlc_section, text="vlc.exe Path:").grid(row=0, column=0, sticky="w")
        self.vlc_path_var = tk.StringVar()
        ttk.Entry(vlc_section, textvariable=self.vlc_path_var, width=50).grid(row=0, column=1, sticky="ew", padx=5)
        ttk.Button(vlc_section, text="Browse vlc.exe...", command=self.select_vlc_path).grid(row=0, column=2)
        
        ttk.Button(vlc_section, text="Test VLC", command=self.test_vlc).grid(row=1, column=0, columnspan=3, pady=10)
        
        ttk.Label(vlc_section, text="Status:", foreground="gray").grid(row=2, column=0, sticky="w")
        self.vlc_status_label = ttk.Label(vlc_section, text="Not initialized", foreground="red")
        self.vlc_status_label.grid(row=2, column=1, sticky="w")
        
        # ===== Icaros Section =====
        icaros_section = ttk.LabelFrame(software_frame, text="🎬 Icaros (Windows Thumbnails)", padding="10")
        icaros_section.pack(fill=tk.X, pady=10)
        icaros_section.columnconfigure(1, weight=1)
        
        ttk.Label(icaros_section, text="Icaros Status:").grid(row=0, column=0, sticky="w")
        self.icaros_status_label = ttk.Label(icaros_section, text="Not installed", foreground="red")
        self.icaros_status_label.grid(row=0, column=1, sticky="w", padx=5)
        
        ttk.Label(icaros_section, text="carosConfig.exe path:").grid(row=1, column=0, sticky="w")
        self.icaros_path_var = tk.StringVar(value="")
        icaros_path_entry = ttk.Entry(icaros_section, textvariable=self.icaros_path_var, width=50)
        icaros_path_entry.grid(row=1, column=1, sticky="ew", padx=5)
        
        icaros_button_frame = ttk.Frame(icaros_section)
        icaros_button_frame.grid(row=2, column=0, columnspan=2, sticky="ew", pady=5)
        icaros_button_frame.columnconfigure(0, weight=1)
        icaros_button_frame.columnconfigure(1, weight=1)
        
        ttk.Button(icaros_button_frame, text="📥 Download Icaros", command=self.download_icaros).pack(side=tk.LEFT, padx=2, fill=tk.X, expand=True)
        ttk.Button(icaros_button_frame, text="📂 Browse carosConfig.exe", command=self.select_icaros_path).pack(side=tk.LEFT, padx=2, fill=tk.X, expand=True)
        
        # Save button at bottom
        button_frame = ttk.Frame(software_frame)
        button_frame.pack(fill=tk.X, pady=20)
        ttk.Button(button_frame, text="💾 Save", command=self.save_config).pack(side=tk.LEFT, padx=5)
    
    def setup_config_tab(self, parent):
        """Settings tab with frame extraction and output options"""
        # Create a canvas with scrollbar for scrollable content
        canvas = tk.Canvas(parent, bg="white", highlightthickness=0)
        scrollbar = ttk.Scrollbar(parent, orient=tk.VERTICAL, command=canvas.yview)
        
        config_frame = ttk.Frame(canvas, padding="10")
        config_frame.pack(fill=tk.BOTH, expand=True)
        
        # Bind the scroll event
        def on_mousewheel(event):
            canvas.yview_scroll(int(-1*(event.delta/120)), "units")
        canvas.bind_all("<MouseWheel>", on_mousewheel)
        
        canvas.create_window((0, 0), window=config_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        
        # Update scroll region when frame changes
        def on_frame_configure(event=None):
            canvas.configure(scrollregion=canvas.bbox("all"))
        config_frame.bind("<Configure>", on_frame_configure)
        
        canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # ===== Frames Section =====
        extract_section = ttk.LabelFrame(config_frame, text="Frames", padding="10")
        extract_section.pack(fill=tk.X, pady=10)
        extract_section.columnconfigure(1, weight=1)
        
        ttk.Label(extract_section, text="Count:").grid(row=0, column=0, sticky="w")
        self.num_frames_var = tk.IntVar(value=10)
        ttk.Spinbox(extract_section, from_=2, to=20, textvariable=self.num_frames_var, width=10).grid(row=0, column=1, sticky="w", padx=5)
        
        ttk.Label(extract_section, text="Percentages:").grid(row=1, column=0, sticky="nw")
        ttk.Entry(extract_section, textvariable=self.percentages_var, width=50).grid(row=1, column=1, sticky="ew", padx=5)
        
        size_section = ttk.LabelFrame(config_frame, text="Output Size", padding="10")
        size_section.pack(fill=tk.X, pady=10)
        size_section.columnconfigure(1, weight=1)
        
        ttk.Label(size_section, text="Width:").grid(row=0, column=0, sticky="w")
        ttk.Spinbox(size_section, from_=320, to=3840, textvariable=self.width_var, width=10).grid(row=0, column=1, sticky="w", padx=5)
        
        ttk.Label(size_section, text="Height:").grid(row=1, column=0, sticky="w")
        ttk.Spinbox(size_section, from_=180, to=2160, textvariable=self.height_var, width=10).grid(row=1, column=1, sticky="w", padx=5)
        
        # Text Preferences
        text_section = ttk.LabelFrame(config_frame, text="Default Text Settings", padding="10")
        text_section.pack(fill=tk.X, pady=10)
        text_section.columnconfigure(1, weight=1)
        
        ttk.Label(text_section, text="Default Text:").grid(row=0, column=0, sticky="w")
        self.default_text_var = tk.StringVar(value="JPG")
        ttk.Entry(text_section, textvariable=self.default_text_var, width=50).grid(row=0, column=1, sticky="ew", padx=5)
        
        ttk.Label(text_section, text="Default Size:").grid(row=1, column=0, sticky="w")
        self.default_size_var = tk.IntVar(value=30)
        ttk.Spinbox(text_section, from_=10, to=100, textvariable=self.default_size_var, width=10).grid(row=1, column=1, sticky="w", padx=5)
        
        ttk.Label(text_section, text="Default Position:").grid(row=2, column=0, sticky="w")
        self.default_pos_var = tk.StringVar(value="down-center")
        ttk.Combobox(text_section, textvariable=self.default_pos_var,
                    values=["top-left", "top-center", "top-right", "down-left", "down-center", "down-right", "center"],
                    state="readonly", width=20).grid(row=2, column=1, sticky="ew", padx=5)
        
        ttk.Label(text_section, text="Default Text Color:").grid(row=3, column=0, sticky="w")
        self.default_text_color_var = tk.StringVar(value="#FFFF00")
        ttk.Entry(text_section, textvariable=self.default_text_color_var, width=50).grid(row=3, column=1, sticky="ew", padx=5)
        
        ttk.Label(text_section, text="Default BG Color:").grid(row=4, column=0, sticky="w")
        self.default_bg_color_var = tk.StringVar(value="#000000")
        ttk.Entry(text_section, textvariable=self.default_bg_color_var, width=50).grid(row=4, column=1, sticky="ew", padx=5)
        
        button_frame = ttk.Frame(config_frame)
        button_frame.pack(fill=tk.X, pady=20)
        
        ttk.Button(button_frame, text="💾 Save", command=self.save_config).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="🔄 Reset", command=self.reset_config).pack(side=tk.LEFT, padx=5)
    
    def check_ffmpeg(self):
        """Checks FFmpeg"""
        try:
            subprocess.run(['ffmpeg', '-version'], capture_output=True, check=True, timeout=5)
            self.log("[OK] FFmpeg detected")
        except:
            self.log("[ERROR] FFmpeg not found")
    
    def select_folder(self):
        """Selects a folder"""
        folder = filedialog.askdirectory(title="Select a folder with videos")
        if folder:
            self.current_folder = folder
            self.folder_label.config(text=os.path.basename(folder), foreground="green")
            self.log(f"[OK] Folder: {os.path.basename(folder)}")
            
            script_dir = os.path.dirname(os.path.abspath(__file__))
            self.screenshots_dir = os.path.join(script_dir, ".screenshots")
            os.makedirs(self.screenshots_dir, exist_ok=True)
            
            self.load_videos_gallery()
    
    def load_videos_gallery(self):
        """Loads the video gallery"""
        for thumb in self.gallery_thumbnails:
            thumb.destroy()
        self.gallery_thumbnails.clear()
        
        video_extensions = ('.mp4', '.mkv', '.avi', '.mov', '.flv', '.wmv', '.webm', '.m4v')
        videos = []
        
        for file in sorted(os.listdir(self.current_folder)):
            if file.lower().endswith(video_extensions):
                videos.append(file)
        
        self.log(f"[OK] {len(videos)} video(s) found")
        
        for video_name in videos:
            video_path = os.path.join(self.current_folder, video_name)
            thread = threading.Thread(target=self._create_gallery_thumbnail_thread, args=(video_path, video_name), daemon=True)
            thread.start()
    
    def _create_gallery_thumbnail_thread(self, video_path, video_name):
        """Thread to create a thumbnail"""
        self.create_gallery_thumbnail(video_path, video_name)
    
    def create_gallery_thumbnail(self, video_path, video_name):
        """Creates a gallery thumbnail"""
        try:
            cmd_duration = [
                'ffprobe', '-v', 'error', '-show_entries', 'format=duration',
                '-of', 'default=noprint_wrappers=1:nokey=1', video_path
            ]
            
            result = subprocess.run(cmd_duration, capture_output=True, text=True, timeout=5, encoding='utf-8', errors='replace')
            duration = float(result.stdout.strip())
            
            timestamp = (duration * 25) / 100
            script_dir = os.path.dirname(os.path.abspath(__file__))
            temp_thumb = os.path.join(script_dir, f".temp_{os.path.basename(video_path)}_25.jpg")
            
            cmd = [
                'ffmpeg', '-ss', f'{timestamp:.2f}',
                '-i', video_path,
                '-vf', 'scale=150:-1',
                '-vframes', '1',
                '-y',
                temp_thumb
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=10, encoding='utf-8', errors='replace')
            
            if os.path.exists(temp_thumb) and os.path.getsize(temp_thumb) > 0:
                img = Image.open(temp_thumb)
                height = int((img.height / img.width) * 150)
                img = img.resize((150, height), Image.Resampling.LANCZOS)
                
                thumb_frame = tk.Frame(self.gallery_container, bg="gray30", cursor="hand2")
                thumb_frame.pack(fill=tk.X, padx=2, pady=2)
                
                photo = ImageTk.PhotoImage(img)
                img_label = tk.Label(thumb_frame, image=photo, bg="gray30")
                img_label.image = photo
                img_label.pack(fill=tk.BOTH, expand=True)
                
                name_label = tk.Label(thumb_frame, text=video_name[:18], bg="gray30", fg="white", 
                                    font=("", 7), wraplength=145)
                name_label.pack(fill=tk.X, padx=2, pady=1)
                
                def make_click_handler(vpath, vname):
                    def on_click(e):
                        self.select_video(vpath, vname)
                    return on_click
                
                click_handler = make_click_handler(video_path, video_name)
                thumb_frame.bind("<Button-1>", click_handler)
                img_label.bind("<Button-1>", click_handler)
                name_label.bind("<Button-1>", click_handler)
                
                def on_enter(e):
                    thumb_frame.config(bg="gray40")
                def on_leave(e):
                    thumb_frame.config(bg="gray30")
                
                thumb_frame.bind("<Enter>", on_enter)
                thumb_frame.bind("<Leave>", on_leave)
                
                self.gallery_thumbnails.append(thumb_frame)
                
                os.remove(temp_thumb)
                
        except Exception as e:
            self.log(f"[ERROR] Gallery {video_name} : {str(e)[:50]}")
    
    def check_existing_thumbnail(self, video_path):
        """Checks if video already has an embedded thumbnail"""
        try:
            cmd = [
                'ffprobe', '-v', 'error',
                '-select_streams', 'v:0',
                '-show_entries', 'stream=disposition',
                '-of', 'default=noprint_wrappers=1:nokey=1',
                video_path
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, timeout=5, encoding='utf-8', errors='replace')
            output = result.stdout.strip()
            
            # Check if it contains attached_pic disposition
            self.has_existing_thumbnail = 'attached_pic=1' in output
            
            if self.has_existing_thumbnail:
                self.info_thumbnail.config(text="Yes", foreground="green")
                self.log("[INFO] Existing thumbnail found")
            else:
                self.info_thumbnail.config(text="No", foreground="red")
                self.log("[INFO] No existing thumbnail")
                
        except Exception as e:
            self.log(f"[ERROR] Checking thumbnail : {str(e)}")
            self.has_existing_thumbnail = False
            self.info_thumbnail.config(text="?", foreground="gray")
    
    def select_video(self, video_path, video_name):
        """Selects a video"""
        self.video_path = video_path
        self.video_name_label.config(text=f"📹 {video_name}", foreground="blue")
        
        # Clear previous data BUT KEEP gallery
        self.screenshots = {}
        self.video_frames = {}
        self.current_image_pil = None
        self.final_thumbnail_pil = None
        self.progress_var.set(0)
        
        # Clear only screenshots display (not gallery)
        for widget in self.screenshots_widgets.values():
            widget.destroy()
        self.screenshots_widgets.clear()
        
        self.log(f"[INFO] Selected: {video_name}")
        self.log(f"[INFO] Extracting 10 frames...")
        
        # Check if format supports thumbnails
        self.check_thumbnail_support()
        
        # Get video info
        self.get_video_info(video_path)
        
        # Check existing thumbnail
        self.check_existing_thumbnail(video_path)
        
        self.load_screenshots()
        
        thread = threading.Thread(target=self.extract_multiple_frames, daemon=True)
        thread.start()
    
    def get_video_info(self, video_path):
        """Gets video information"""
        try:
            # File size
            size_bytes = os.path.getsize(video_path)
            if size_bytes > 1024*1024:
                size_str = f"{size_bytes / (1024*1024):.1f} MB"
            else:
                size_str = f"{size_bytes / 1024:.1f} KB"
            
            self.info_size.config(text=size_str)
            
            # Duration
            cmd_duration = [
                'ffprobe', '-v', 'error', '-show_entries', 'format=duration',
                '-of', 'default=noprint_wrappers=1:nokey=1', video_path
            ]
            result = subprocess.run(cmd_duration, capture_output=True, text=True, timeout=5, encoding='utf-8', errors='replace')
            duration = float(result.stdout.strip())
            self.info_duration.config(text=self.format_time(duration))
            
            # Format
            ext = os.path.splitext(video_path)[1].replace(".", "").upper()
            self.info_format.config(text=ext)
            
            self.video_info = {
                'size': size_str,
                'duration': self.format_time(duration),
                'format': ext
            }
            
        except Exception as e:
            self.log(f"[ERROR] Getting video info : {str(e)}")
    
    def load_screenshots(self):
        """Clear screenshots for fresh start when selecting new video"""
        self.screenshots.clear()
        for widget in self.screenshots_widgets.values():
            widget.destroy()
        self.screenshots_widgets.clear()
        self.display_screenshots()  # Show empty gallery
    
    def save_screenshot(self):
        """Saves a screenshot from VLC current position"""
        if not self.video_path:
            messagebox.showerror("Error", "Select a video first")
            return
        
        if not self.screenshots_dir:
            messagebox.showerror("Error", "Select a folder with videos first")
            return
        
        if not self.vlc_player:
            messagebox.showerror("Error", "VLC player not initialized")
            return
        
        try:
            media_player = self.vlc_player.get_media_player()
            current_time = media_player.get_time() / 1000
            
            if current_time < 0:
                messagebox.showerror("Error", "Video not playing. Press Play first.")
                return
            
            video_base = os.path.splitext(os.path.basename(self.video_path))[0]
            filename = f"{video_base}_{current_time:.2f}.jpg"
            screenshot_path = os.path.join(self.screenshots_dir, filename)
            
            # Ensure directory exists
            os.makedirs(self.screenshots_dir, exist_ok=True)
            
            # Use FFmpeg to capture frame at current time
            cmd = [
                'ffmpeg',
                '-ss', str(current_time),  # Seek to this time
                '-i', self.video_path,
                '-vframes', '1',  # Capture only 1 frame
                '-q:v', '2',  # Quality (1-31, lower is better)
                '-y',  # Overwrite
                screenshot_path
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace')
            
            if result.returncode != 0:
                raise Exception(f"FFmpeg error capturing frame")
            
            self.screenshots[current_time] = screenshot_path
            self.log(f"[OK] Screenshot saved at {self.format_time(current_time)}")
            self.display_screenshots()
            self.update_preview_live()
            
        except Exception as e:
            self.log(f"[ERROR] Save screenshot : {str(e)}")
            messagebox.showerror("Error", f"Error : {str(e)}")
    
    def display_screenshots(self):
        """Displays screenshots in 2-column grid"""
        for widget in self.screenshots_widgets.values():
            widget.destroy()
        self.screenshots_widgets.clear()
        
        # Configure grid columns for screenshots container
        self.screenshots_container.columnconfigure(0, weight=1)
        self.screenshots_container.columnconfigure(1, weight=1)
        
        row = 0
        col = 0
        
        for idx, timestamp in enumerate(sorted(self.screenshots.keys())):
            screenshot_path = self.screenshots[timestamp]
            
            try:
                img = Image.open(screenshot_path)
                img.thumbnail((90, 60), Image.Resampling.LANCZOS)
                
                photo = ImageTk.PhotoImage(img)
                
                frame_widget = tk.Frame(self.screenshots_container, bg="gray40", cursor="hand2", relief=tk.SUNKEN, bd=2)
                frame_widget.grid(row=row, column=col, sticky="ew", padx=2, pady=2)
                
                img_label = tk.Label(frame_widget, image=photo, bg="gray40")
                img_label.image = photo
                img_label.pack(padx=3, pady=3)
                
                time_label = tk.Label(frame_widget, text=f"{self.format_time(timestamp)}", bg="gray40", fg="white", font=("", 7))
                time_label.pack(padx=3, pady=2)
                
                def make_click_handler(ts):
                    def on_click(e):
                        self.seek_to_screenshot(ts)
                    return on_click
                
                def make_right_click_handler(ts, path):
                    def on_right_click(e):
                        self.delete_screenshot(ts, path)
                    return on_right_click
                
                frame_widget.bind("<Button-1>", make_click_handler(timestamp))
                frame_widget.bind("<Button-3>", make_right_click_handler(timestamp, screenshot_path))
                img_label.bind("<Button-1>", make_click_handler(timestamp))
                img_label.bind("<Button-3>", make_right_click_handler(timestamp, screenshot_path))
                time_label.bind("<Button-1>", make_click_handler(timestamp))
                time_label.bind("<Button-3>", make_right_click_handler(timestamp, screenshot_path))
                
                self.screenshots_widgets[timestamp] = frame_widget
                
                # Move to next row after 2 columns
                col += 1
                if col >= 2:
                    col = 0
                    row += 1
                
            except Exception as e:
                self.log(f"[ERROR] Displaying screenshot : {str(e)}")
        
        # Update preview when screenshots change
        self.update_preview_live()
    
    def seek_to_screenshot(self, timestamp):
        """Seeks to screenshot time"""
        if self.vlc_player:
            try:
                media_player = self.vlc_player.get_media_player()
                ms_time = int(timestamp * 1000)
                media_player.set_time(ms_time)
                self.log(f"[OK] Playing at {self.format_time(timestamp)}")
            except Exception as e:
                self.log(f"[ERROR] Seek : {str(e)}")
    
    def delete_screenshot(self, timestamp, filepath):
        """Deletes a screenshot"""
        if messagebox.askyesno("Confirmation", f"Delete screenshot at {self.format_time(timestamp)} ?"):
            try:
                os.remove(filepath)
                del self.screenshots[timestamp]
                self.log(f"[OK] Screenshot deleted at {self.format_time(timestamp)}")
                self.display_screenshots()
                self.update_preview_live()
            except Exception as e:
                self.log(f"[ERROR] Delete screenshot : {str(e)}")
                self.log(f"[ERROR] Deletion : {str(e)}")
    
    def extract_multiple_frames(self):
        """Extracts 10 frames"""
        if not self.video_path:
            return
        
        try:
            cmd_duration = [
                'ffprobe', '-v', 'error', '-show_entries', 'format=duration',
                '-of', 'default=noprint_wrappers=1:nokey=1', self.video_path
            ]
            
            result = subprocess.run(cmd_duration, capture_output=True, text=True, timeout=5, encoding='utf-8', errors='replace')
            self.video_duration = float(result.stdout.strip())
            self.log(f"[OK] Duration: {self.format_time(self.video_duration)}")
            
            try:
                percentages_str = self.percentages_var.get()
                percentages = [int(p.strip()) for p in percentages_str.split(',')]
            except:
                percentages = [0, 10, 20, 30, 40, 50, 60, 70, 80, 90]
            
            self.video_frames = {}
            script_dir = os.path.dirname(os.path.abspath(__file__))
            temp_dir = os.path.join(script_dir, ".temp_frames")
            os.makedirs(temp_dir, exist_ok=True)
            self.temp_dir = temp_dir
            
            for percent in percentages:
                timestamp = (self.video_duration * percent) / 100
                output_file = os.path.join(temp_dir, f"frame_{percent}.jpg")
                
                cmd = [
                    'ffmpeg', '-ss', f'{timestamp:.2f}',
                    '-i', self.video_path,
                    '-vframes', '1',
                    '-q:v', '2',  # Better quality
                    '-y',
                    output_file
                ]
                
                result = subprocess.run(cmd, capture_output=True, text=True, 
                                      encoding='utf-8', errors='replace', timeout=10)
                
                if os.path.exists(output_file) and os.path.getsize(output_file) > 100:
                    self.video_frames[percent] = output_file
                    self.log(f"[OK] Frame {percent}% extracted")
                else:
                    self.log(f"[WARNING] Frame {percent}% extraction may have failed - file empty or missing")
            
            self.display_frame_thumbnails()
            self.select_frame_percent(25)
            
            self.load_video_in_vlc()
            
            # Auto-generate thumbnail if not already present
            self.root.after(500, self.auto_generate_thumbnail)
            
        except Exception as e:
            self.log(f"[ERROR] Extraction : {str(e)}")
            messagebox.showerror("Error", f"Error : {str(e)}")
    
    def load_video_in_vlc(self):
        """Loads video in VLC player"""
        if not VLC_AVAILABLE or not self.vlc_instance:
            self.log("[WARNING] VLC not available")
            return
        
        try:
            if self.vlc_player:
                self.vlc_player.stop()
            
            self.vlc_player = self.vlc_instance.media_list_player_new()
            media_list = self.vlc_instance.media_list_new()
            
            media = self.vlc_instance.media_new(self.video_path)
            media_list.add_media(media)
            
            self.vlc_player.set_media_list(media_list)
            
            media_player = self.vlc_player.get_media_player()
            
            if sys.platform.startswith('linux') or sys.platform == 'darwin':
                media_player.set_xwindow(self.vlc_canvas.winfo_id())
            else:
                media_player.set_hwnd(self.vlc_canvas.winfo_id())
            
            self.log("[OK] Video loaded in VLC")
            
            # Auto-play the video
            self.vlc_player.play()
            self.log("[OK] Auto-playing video")
            
        except Exception as e:
            self.log(f"[ERROR] Loading VLC : {str(e)}")
    
    def display_frame_thumbnails(self):
        """Displays 10 frame thumbnails"""
        for percent in sorted(self.video_frames.keys()):
            frame_path = self.video_frames[percent]
            
            if percent in self.frame_images:
                try:
                    img = Image.open(frame_path)
                    img.thumbnail((70, 40), Image.Resampling.LANCZOS)
                    
                    photo = ImageTk.PhotoImage(img)
                    
                    btn = self.frame_images[percent]
                    btn.config(image=photo, text="")
                    btn.image = photo
                    
                except Exception as e:
                    self.log(f"[ERROR] Frame {percent}% : {str(e)[:30]}")
    
    def select_frame_percent(self, percent):
        """Selects a frame percentage and seeks video"""
        self.current_frame_percent = percent
        
        if percent in self.video_frames:
            frame_path = self.video_frames[percent]
            
            if os.path.exists(frame_path):
                img = Image.open(frame_path)
                img = img.convert('RGB')
                
                # Check if image is too dark (might indicate extraction failure)
                img_array = img.tobytes()
                avg_brightness = sum(img_array) / len(img_array) if img_array else 0
                is_too_dark = avg_brightness < 20  # Threshold for "black image"
                
                if is_too_dark:
                    self.log(f"[WARNING] Frame {percent}% appears black - format may not be supported")
                    self.log(f"[INFO] Try taking a screenshot from the video player instead")
                
                display_img = img.copy()
                display_img.thumbnail((300, 250), Image.Resampling.LANCZOS)
                
                photo = ImageTk.PhotoImage(display_img)
                self.vlc_canvas.create_image(0, 0, image=photo, anchor="nw")
                self.vlc_canvas.image = photo
                
                self.current_image_pil = img
                self.thumb_path = frame_path
                
                self.timeline_var.set(percent)
                timestamp = (self.video_duration * percent) / 100
                self.time_label.config(text=f"{self.format_time(timestamp)} / {self.format_time(self.video_duration)}")
                
                # Seek VLC to this position
                if self.vlc_player:
                    self.vlc_seeking = True
                    media_player = self.vlc_player.get_media_player()
                    media_player.set_time(int(timestamp * 1000))
                    
                    # Resume playback after seek
                    if not media_player.is_playing():
                        self.vlc_player.play()
                        self.log(f"[OK] Seeked to {self.format_time(timestamp)} and resumed playback")
                    else:
                        self.log(f"[OK] Seeked to {self.format_time(timestamp)}")
                    
                    self.vlc_seeking = False
                
                self.update_preview_live()
    
    def on_timeline_change(self, value):
        """When timeline changes"""
        if self.vlc_seeking:
            return
        
        self.vlc_seeking = True
        try:
            percent = float(value)
            
            if self.video_frames:
                closest_percent = min(self.video_frames.keys(), key=lambda x: abs(x - percent))
                
                if closest_percent != self.current_frame_percent:
                    self.select_frame_percent(closest_percent)
                
                if self.vlc_player:
                    media_player = self.vlc_player.get_media_player()
                    timestamp = (self.video_duration * percent) / 100
                    ms_time = int(timestamp * 1000)
                    media_player.set_time(ms_time)
        finally:
            self.vlc_seeking = False
    
    def play_video(self):
        """Starts video playback"""
        if not self.video_path:
            messagebox.showerror("Error", "Select a video")
            return
        
        try:
            if self.vlc_player:
                self.vlc_player.play()
                self.play_button.config(state=tk.DISABLED)
                self.log("[OK] Playing")
                
                self.update_timeline()
        except Exception as e:
            self.log(f"[ERROR] Playing : {str(e)}")
    
    def update_timeline(self):
        """Updates the timeline"""
        if self.vlc_player and self.vlc_player.is_playing():
            media_player = self.vlc_player.get_media_player()
            duration = media_player.get_length() / 1000
            current_time = media_player.get_time() / 1000
            
            if duration > 0:
                percent = (current_time / duration) * 100
                self.vlc_seeking = True
                self.timeline_var.set(percent)
                self.vlc_seeking = False
                self.time_label.config(text=f"{self.format_time(current_time)} / {self.format_time(duration)}")
            
            self.root.after(100, self.update_timeline)
        else:
            self.play_button.config(state=tk.NORMAL)
    
    def pause_video(self):
        """Pauses playback"""
        if self.vlc_player:
            self.vlc_player.pause()
            self.log("[INFO] Pause")
    
    def stop_video(self):
        """Stops playback"""
        if self.vlc_player:
            self.vlc_player.stop()
            self.play_button.config(state=tk.NORMAL)
            self.log("[INFO] Stop")
    
    def on_volume_change(self, value):
        """Changes volume"""
        if self.vlc_player:
            media_player = self.vlc_player.get_media_player()
            media_player.audio_set_volume(int(float(value)))
    
    def safe_subprocess(self, cmd, capture_output=True, timeout=10, text=True):
        """Safe subprocess call with UTF-8 encoding"""
        try:
            return subprocess.run(cmd, capture_output=capture_output, 
                                timeout=timeout, text=text,
                                encoding='utf-8', errors='replace')
        except subprocess.TimeoutExpired:
            self.log(f"[WARNING] Command timeout: {' '.join(cmd[:2])}")
            return None
        except Exception as e:
            self.log(f"[ERROR] Subprocess error: {str(e)}")
            return None
        """Seeks video by N seconds (positive or negative)"""
        if not self.vlc_player:
            return
        
        try:
            media_player = self.vlc_player.get_media_player()
            current_time = media_player.get_time() / 1000  # Convert to seconds
            new_time = max(0, current_time + seconds)  # Don't go negative
            
            media_player.set_time(int(new_time * 1000))  # Convert back to milliseconds
            self.log(f"[OK] Seeked to {self.format_time(new_time)}")
        except Exception as e:
            self.log(f"[ERROR] Seek failed: {str(e)}")
        """Formats seconds to MM:SS"""
        m, s = divmod(int(seconds), 60)
        return f"{m:02d}:{s:02d}"
    
    def choose_text_color(self):
        """Chooses text color"""
        color = colorchooser.askcolor(color=self.text_color)
        if color[0]:
            self.text_color = tuple(int(c) for c in color[0])
            self.color_label.config(bg=color[1])
            self.log(f"[OK] Text color: {color[1]}")
            self.update_preview_live()
    
    def choose_text_bg_color(self):
        """Chooses text background color"""
        color = colorchooser.askcolor(color=self.text_bg_color)
        if color[0]:
            self.text_bg_color = tuple(int(c) for c in color[0])
            self.color_bg_label.config(bg=color[1])
            self.log(f"[OK] Background color: {color[1]}")
            self.update_preview_live()
    
    def create_montage(self, screenshots_list):
        """Creates a 2x2 montage of screenshots"""
        if len(screenshots_list) == 0:
            return None
        
        if len(screenshots_list) == 1:
            return Image.open(screenshots_list[0]).convert('RGB')
        
        images = []
        for path in screenshots_list[:4]:
            try:
                img = Image.open(path).convert('RGB')
                images.append(img)
            except:
                pass
        
        if not images:
            return None
        
        resized = []
        for img in images:
            resized.append(img.resize((self.FRAME_WIDTH, self.FRAME_HEIGHT), Image.Resampling.LANCZOS))
            self.log(f"[INFO] Resized frame to {self.FRAME_WIDTH}x{self.FRAME_HEIGHT}")
        
        montage = Image.new('RGB', (self.MONTAGE_WIDTH, self.MONTAGE_HEIGHT), color='black')
        
        self.log(f"[INFO] Created montage {self.MONTAGE_WIDTH}x{self.MONTAGE_HEIGHT}")
        
        positions = [(0, 0), (self.FRAME_WIDTH, 0), (0, self.FRAME_HEIGHT), (self.FRAME_WIDTH, self.FRAME_HEIGHT)]
        
        for i, pos in enumerate(positions):
            if i < len(resized):
                montage.paste(resized[i], pos)
        
        return montage
    
    def update_preview_live(self):
        """Updates preview in real-time - prioritizes screenshots"""
        # Log current parameters when preview updates (on parameter change)
        try:
            text = self.overlay_text.get() if hasattr(self, 'overlay_text') else ""
            width = self.width_var.get() if hasattr(self, 'width_var') else 1280
            height = self.height_var.get() if hasattr(self, 'height_var') else 720
            
            # Only log if user is actively changing parameters (has text or non-default size)
            if text or width != 1280 or height != 720:
                self.log(f"[INFO] Preview: Text='{text}' Output={width}x{height}")
        except:
            pass
        
        # PRIORITIZE SCREENSHOTS FIRST - they are always cleaner
        if len(self.screenshots) > 0:
            # Use screenshots (even just one)
            if len(self.screenshots) == 1:
                screenshot_path = list(self.screenshots.values())[0]
                preview = Image.open(screenshot_path).convert('RGB')
                self.log("[INFO] Using screenshot for thumbnail preview")
            else:
                screenshots_paths = list(self.screenshots.values())
                preview = self.create_montage(screenshots_paths)
                if not preview:
                    return
                self.log("[INFO] Using screenshot montage for thumbnail preview")
        elif self.current_image_pil:
            # Fallback to current image if available
            preview = self.current_image_pil.copy()
        else:
            # No image available
            preview = Image.new('RGB', (640, 360), color='black')
        
        text = self.overlay_text.get()
        if text:
            # Use text_size from config/state, not from spinbox
            text_size = self.text_size
            
            draw = ImageDraw.Draw(preview)
            
            try:
                font = ImageFont.truetype("arial.ttf", text_size)
            except:
                try:
                    font = ImageFont.truetype("C:\\Windows\\Fonts\\arial.ttf", text_size)
                except:
                    font = ImageFont.load_default()
            
            bbox = draw.textbbox((0, 0), text, font=font)
            text_width = bbox[2] - bbox[0]
            text_height = bbox[3] - bbox[1]
            
            img_width, img_height = preview.size
            # Use text_position from config/state, not from position_var
            position = self.text_position
            
            positions = {
                "top-left": (10, 10),
                "top-center": ((img_width - text_width) // 2, 10),
                "top-right": (img_width - text_width - 10, 10),
                "down-left": (10, img_height - text_height - 10),
                "down-center": ((img_width - text_width) // 2, img_height - text_height - 10),
                "down-right": (img_width - text_width - 10, img_height - text_height - 10),
                "center": ((img_width - text_width) // 2, (img_height - text_height) // 2)
            }
            
            pos = positions.get(position, positions["down-center"])
            
            # Draw background for text
            bg_padding = 5
            draw.rectangle(
                [pos[0] - bg_padding, pos[1] - bg_padding, pos[0] + text_width + bg_padding, pos[1] + text_height + bg_padding],
                fill=self.text_bg_color
            )
            
            draw.text(pos, text, font=font, fill=self.text_color)
        
        display_img = preview.copy()
        # Adapt thumbnail to available space in final_thumb_label
        available_width = self.final_thumb_label.winfo_width()
        available_height = self.final_thumb_label.winfo_height()
        
        # Use 90% of available space, minimum 150x80, fallback if not rendered yet
        if available_width > 50 and available_height > 50:
            target_width = max(150, int(available_width * 0.9))
            target_height = max(80, int(available_height * 0.9))
            display_img.thumbnail((target_width, target_height), Image.Resampling.LANCZOS)
        else:
            display_img.thumbnail((200, 110), Image.Resampling.LANCZOS)
        
        photo = ImageTk.PhotoImage(display_img)
        self.final_thumb_label.config(image=photo)
        self.final_thumb_label.image = photo
        
        self.final_thumbnail_pil = preview
        
        # Update button states based on thumbnail support and image availability
        self.check_thumbnail_support()
    
    def apply_thumbnail(self):
        """Applies the thumbnail - embeds it directly in the video file"""
        if not self.video_path or not self.final_thumbnail_pil:
            messagebox.showerror("Error", "Select a video and create a thumbnail first")
            return
        
        self.log(f"[INFO] ===== EMBEDDING THUMBNAIL =====")
        
        output_path = os.path.splitext(self.video_path)[0] + "_new" + os.path.splitext(self.video_path)[1]
        script_dir = os.path.dirname(os.path.abspath(__file__))
        temp_thumb = os.path.join(script_dir, "temp_thumb.jpg")
        
        def apply():
            try:
                self.log("[INFO] Creating final image...")
                
                final_image = self.final_thumbnail_pil.copy()
                output_width = self.width_var.get()
                output_height = self.height_var.get()
                final_image = final_image.resize((output_width, output_height), Image.Resampling.LANCZOS)
                self.log(f"[INFO] Resized to {output_width}x{output_height}")
                
                final_image.save(temp_thumb, quality=95)
                self.log("[OK] Thumbnail image ready")
                self.progress_var.set(20)
                
                self.log("[INFO] Embedding thumbnail in video...")
                
                # Use FFmpeg to embed thumbnail
                cmd = [
                    'ffmpeg',
                    '-i', self.video_path,
                    '-i', temp_thumb,
                    '-map', '0',
                    '-map', '1:0',
                    '-c', 'copy',
                    '-c:a', 'copy',
                    '-disposition:v:1', 'attached_pic',
                    '-y',
                    output_path
                ]
                
                result = subprocess.run(cmd, capture_output=True, text=True, timeout=120, encoding='utf-8', errors='replace')
                self.progress_var.set(80)
                
                if result.returncode != 0:
                    raise Exception(f"FFmpeg error: {result.stderr[:300]}")
                
                os.remove(temp_thumb)
                
                # Backup original
                backup_path = self.video_path + ".backup"
                if not os.path.exists(backup_path):
                    shutil.copy(self.video_path, backup_path)
                    self.log("[OK] Backup created")
                
                # Replace original with new file
                shutil.move(output_path, self.video_path)
                
                self.progress_var.set(100)
                self.log("[SUCCESS] Thumbnail embedded!")
                
                # Auto-refresh cache
                self.log("[INFO] Auto-refreshing Windows cache...")
                self.root.after(1000, self.refresh_windows_thumbnails_silent)
                
                messagebox.showinfo("Success", 
                    f"Thumbnail embedded in:\n{os.path.basename(self.video_path)}\n\n"
                    f"Cache is being refreshed...\n"
                    f"Your thumbnail will appear in Explorer shortly!")
                
            except Exception as e:
                self.log(f"[ERROR] {str(e)}")
                messagebox.showerror("Error", f"Error: {str(e)}")
                if os.path.exists(temp_thumb):
                    os.remove(temp_thumb)
                if os.path.exists(output_path):
                    os.remove(output_path)
                self.progress_var.set(0)
        
        thread = threading.Thread(target=apply, daemon=True)
        thread.start()
    
    def reencode_with_thumbnail(self):
        """Reencodes video with thumbnail and converts unsupported formats to MP4"""
        if not self.video_path or not self.final_thumbnail_pil:
            messagebox.showerror("Error", "Select a video and create a final thumbnail")
            return
        
        self.log(f"[INFO] ===== REENCODING WITH THUMBNAIL =====")
        
        # Check format and convert if needed
        base_path = os.path.splitext(self.video_path)[0]
        ext = os.path.splitext(self.video_path)[1].lower()
        
        # Supported formats for embedded thumbnails
        supported_formats = ['.mp4', '.m4v', '.mkv', '.avi', '.webm']
        
        # If format not supported, convert to MP4
        if ext not in supported_formats:
            self.log(f"[INFO] Format {ext} not supported - will convert to MP4")
            output_path = base_path + "_thumbnailed.mp4"
            target_format = ".mp4"
        else:
            output_path = base_path + "_thumbnailed" + ext
            target_format = ext
        
        script_dir = os.path.dirname(os.path.abspath(__file__))
        temp_thumb = os.path.join(script_dir, "temp_thumb.jpg")
        
        def reencode():
            try:
                self.log("[INFO] Creating final image...")
                
                final_image = self.final_thumbnail_pil.copy()
                output_width = self.width_var.get()
                output_height = self.height_var.get()
                final_image = final_image.resize((output_width, output_height), Image.Resampling.LANCZOS)
                self.log(f"[INFO] Resized to {output_width}x{output_height}")
                
                final_image.save(temp_thumb, quality=95)
                self.log("[OK] Image saved")
                self.progress_var.set(10)
                
                # Get original video bitrate and audio codec
                self.log("[INFO] Analyzing source video...")
                cmd_probe = [
                    'ffprobe', '-v', 'error',
                    '-select_streams', 'v:0',
                    '-show_entries', 'stream=bit_rate',
                    '-of', 'default=noprint_wrappers=1:nokey=1',
                    self.video_path
                ]
                result = subprocess.run(cmd_probe, capture_output=True, text=True, encoding='utf-8', errors='replace')
                bitrate_str = result.stdout.strip()
                
                # Convert bitrate from bits to kbits if needed
                try:
                    bitrate_bits = int(bitrate_str)
                    bitrate = f"{bitrate_bits // 1000}k"  # Convert to kbps
                except:
                    bitrate = '5000k'
                
                self.log(f"[INFO] Using bitrate: {bitrate}")
                
                if ext not in supported_formats:
                    self.log(f"[INFO] Converting {ext} to MP4 with thumbnail...")
                else:
                    self.log("[INFO] Reencoding video with thumbnail...")
                
                # Simplified FFmpeg command
                cmd = [
                    'ffmpeg',
                    '-i', self.video_path,
                    '-i', temp_thumb,
                    '-c:v', 'libx264',
                    '-b:v', bitrate,
                    '-c:a', 'aac',
                    '-b:a', '128k',
                    '-preset', 'medium',
                    '-map', '0:v:0',
                    '-map', '0:a:0',
                    '-map', '1:0',
                    '-disposition:v:1', 'attached_pic',
                    '-y',
                    output_path
                ]
                
                result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace')
                self.progress_var.set(80)
                
                if result.returncode != 0:
                    raise Exception(f"FFmpeg error: {result.stderr[:200]}")
                
                os.remove(temp_thumb)
                
                self.progress_var.set(100)
                self.log("[SUCCESS] Reencoding complete !")
                self.log(f"[INFO] File saved as: {os.path.basename(output_path)}")
                
                # Auto-refresh cache
                self.log("[INFO] Auto-refreshing Windows cache...")
                self.root.after(1000, self.refresh_windows_thumbnails_silent)
                
                messagebox.showinfo("Success", f"Reencoding complete !\n\nFile saved as: {os.path.basename(output_path)}\n\nCache is being refreshed...")
                
            except Exception as e:
                self.log(f"[ERROR] {str(e)}")
                messagebox.showerror("Error", f"Error : {str(e)}")
                if os.path.exists(temp_thumb):
                    os.remove(temp_thumb)
                if os.path.exists(output_path):
                    os.remove(output_path)
                self.progress_var.set(0)
        
        thread = threading.Thread(target=reencode, daemon=True)
        thread.start()
    
    def select_ffmpeg_path(self):
        """Selects ffmpeg.exe executable"""
        path = filedialog.askopenfilename(
            title="Select ffmpeg.exe",
            filetypes=[("FFmpeg executable", "ffmpeg.exe"), ("Executable files", "*.exe"), ("All files", "*.*")]
        )
        if path and os.path.exists(path):
            self.ffmpeg_path_var.set(path)
            self.log(f"[OK] FFmpeg path updated: {path}")
            messagebox.showinfo("Success", f"FFmpeg set to:\n{path}")
        elif path:
            messagebox.showerror("Error", "ffmpeg.exe not found")
    
    def select_vlc_path(self):
        """Selects vlc.exe executable"""
        path = filedialog.askopenfilename(
            title="Select vlc.exe",
            filetypes=[("VLC executable", "vlc.exe"), ("Executable files", "*.exe"), ("All files", "*.*")]
        )
        if path and os.path.exists(path):
            self.vlc_path_var.set(path)
            self.log(f"[OK] VLC path updated: {path}")
            messagebox.showinfo("Success", f"VLC set to:\n{path}")
        elif path:
            messagebox.showerror("Error", "vlc.exe not found")
    
    def test_ffmpeg(self):
        """Tests FFmpeg"""
        try:
            subprocess.run(['ffmpeg', '-version'], capture_output=True, check=True, timeout=5)
            messagebox.showinfo("Success", "FFmpeg works !")
            self.log("[OK] FFmpeg OK")
        except:
            messagebox.showerror("Error", "FFmpeg not found")
            self.log("[ERROR] FFmpeg failed")
    
    def test_vlc(self):
        """Tests VLC"""
        if VLC_AVAILABLE and self.vlc_instance:
            messagebox.showinfo("Success", "VLC works !")
            self.vlc_status_label.config(text="✓ Operational", foreground="green")
            self.log("[OK] VLC OK")
        else:
            messagebox.showerror("Error", "VLC not available")
            self.vlc_status_label.config(text="✗ Not available", foreground="red")
            self.log("[ERROR] VLC failed")
    
    def save_config(self):
        """Saves config - simple with pathlib"""
        config = {
            "ffmpeg_path": self.ffmpeg_path_var.get(),
            "vlc_path": self.vlc_path_var.get(),
            "num_frames": self.num_frames_var.get(),
            "percentages": self.percentages_var.get(),
            "width": self.width_var.get(),
            "height": self.height_var.get(),
            "default_text": self.default_text_var.get(),
            "default_size": self.default_size_var.get(),
            "default_position": self.default_pos_var.get(),
            "default_text_color": self.default_text_color_var.get(),
            "default_bg_color": self.default_bg_color_var.get(),
            "icaros_path": self.icaros_path_var.get(),
        }
        
        try:
            # Use pathlib write_text - simpler and more reliable
            config_text = json.dumps(config, indent=2)
            self.config_file.write_text(config_text, encoding='utf-8')
            
            self.log(f"[OK] Configuration saved")
            messagebox.showinfo("Success", f"Saved to:\n{self.config_file}")
            
        except Exception as e:
            self.log(f"[ERROR] Save config: {str(e)}")
            messagebox.showerror("Error", f"Error: {str(e)}")
    
    def load_config(self):
        """Loads config using pathlib"""
        if self.config_file.exists():
            try:
                config_text = self.config_file.read_text(encoding='utf-8')
                config = json.loads(config_text)
                self._pending_config = config
            except:
                self._pending_config = None
        else:
            self._pending_config = None
    
    def apply_pending_config(self):
        """Applies config to all variables and UI"""
        if hasattr(self, '_pending_config') and self._pending_config:
            config = self._pending_config
            if 'ffmpeg_path' in config:
                self.ffmpeg_path_var.set(config['ffmpeg_path'])
            if 'vlc_path' in config:
                self.vlc_path_var.set(config['vlc_path'])
            if 'num_frames' in config:
                self.num_frames_var.set(config['num_frames'])
            if 'percentages' in config:
                self.percentages_var.set(config['percentages'])
            if 'width' in config:
                self.width_var.set(config['width'])
            if 'height' in config:
                self.height_var.set(config['height'])
            if 'default_text' in config:
                self.overlay_text.set(config['default_text'])
                self.default_text_var.set(config['default_text'])
            if 'default_size' in config:
                self.text_size = config['default_size']
                self.default_size_var.set(config['default_size'])
                # Sync UI widget
                if hasattr(self, 'size_spinbox'):
                    self.size_spinbox.set(config['default_size'])
            if 'default_position' in config:
                self.text_position = config['default_position']
                self.default_pos_var.set(config['default_position'])
                # Sync UI widget
                if hasattr(self, 'position_var'):
                    self.position_var.set(config['default_position'])
            if 'icaros_path' in config:
                self.icaros_path_var.set(config['icaros_path'])
            
            self.log("[OK] Configuration loaded")
            
            if VLC_AVAILABLE and self.vlc_instance:
                self.vlc_status_label.config(text="✓ Operational", foreground="green")
    
    def reset_config(self):
        """Resets config"""
        if messagebox.askyesno("Confirmation", "Reset?"):
            if self.config_file.exists():
                self.config_file.unlink()
            self.log("[OK] Configuration reset")
            messagebox.showinfo("Info", "Restart the application")

if __name__ == "__main__":
    root = tk.Tk()
    app = VideoThumbnailChanger(root)
    app.apply_pending_config()
    root.mainloop()