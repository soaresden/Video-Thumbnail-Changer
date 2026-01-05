# 🎬 Video Thumbnail Changer

Complete reimagination from vb.net to Python
A powerful desktop application to **embed custom thumbnails** in your video files with an intuitive UI and real-time preview.

<img width="1497" height="905" alt="image" src="https://github.com/user-attachments/assets/e80b843d-8ee1-48f3-88e6-a29407c0f660" />


## ✨ Features

- 🎞️ **Multi-Frame Extraction** - Extract and browse 10 key frames from your video
- 🎮 **VLC Player Integration** - Watch videos directly in the app with keyboard controls
- 📸 **Screenshot Capture** - Grab high-quality screenshots at any time
- 🖼️ **2x2 Montage Mode** - Create thumbnail grids from 4 screenshots
- ✏️ **Live Text Overlay** - Add customizable text with real-time preview
  - Adjustable font size (10-100px)
  - 7 position options (top/center/bottom, left/center/right)
  - Custom text colors & backgrounds
- 🎯 **Smart Format Conversion** - Automatically convert .mov, .wmv, etc to MP4
- 💾 **Apply or Reencode** - Choose between fast embedding or full re-encoding
- 🖥️ **Configuration Saving** - Save your settings and restore them later
- ⚡ **Hardware Acceleration** - Auto-detect GPU support (NVIDIA, Intel, AMD)

## 🎮 Keyboard Controls

| Key | Action |
|-----|--------|
| **← →** | Seek ±1 second |
| **↑ ↓** | Seek ±5 seconds |
| **Mouse Wheel** | Scroll gallery (works on thumbnails!) |

## 📋 System Requirements

- **Python 3.7+**
- **FFmpeg** (for video processing)
- **FFprobe** (for video analysis)
- **VLC libraries** (python-vlc)
- **Pillow** (image processing)

### 🪟 Windows
```bash
# Install FFmpeg via Chocolatey
choco install ffmpeg

# Or download from: https://ffmpeg.org/download.html
```

### 🍎 macOS
```bash
brew install ffmpeg
```

### 🐧 Linux
```bash
sudo apt-get install ffmpeg
```

## 🚀 Quick Start

### 1️⃣ Clone the repository
```bash
git clone https://github.com/yourusername/video-thumbnail-changer.git
cd video-thumbnail-changer
```

### 2️⃣ Install dependencies
```bash
pip install -r requirements.txt
```

### 3️⃣ Run the application
```bash
python video_thumbnail_changer_v4.9.py
```

## 📖 How to Use

### Basic Workflow 🔄

1. **Load a Video** 📂
   - Click "Select Video"
   - Choose .mp4, .mov, .mkv, .avi, .webm, or .m4v

2. **Extract Frames** 🎞️
   - App automatically extracts 10 key frames
   - Browse them in the gallery

3. **Create Thumbnail** 🖼️
   - Take screenshots at desired moments
   - Or select extracted frames
   - Arrange as single image or 2x2 montage

4. **Add Text** ✏️
   - Type your text
   - Adjust size (10-100px)
   - Choose position & colors
   - See real-time preview

5. **Apply or Reencode** 💾
   - **Apply**: Fast - embeds thumbnail only (supported formats)
   - **Reencode**: Converts format if needed (universal)

6. **Refresh Cache** 🔄 (Optional)
   - Update Windows Explorer thumbnails immediately

### Pro Tips 💡

✅ Use **screenshots** instead of frame extraction for better quality  
✅ The **2x2 montage** keeps the same size as a single image  
✅ **Reencode** is required for .mov files (converts to MP4)  
✅ Check logs for exact file paths and details  
✅ Use **Refresh Cache** if thumbnails don't update in Explorer

## 📊 Supported Formats

| Format | Apply | Reencode |
|--------|-------|----------|
| .mp4 | ✅ | ✅ |
| .m4v | ✅ | ✅ |
| .mkv | ✅ | ✅ |
| .avi | ✅ | ✅ |
| .webm | ✅ | ✅ |
| .mov | ❌ | ✅ (→ MP4) |
| .wmv | ❌ | ✅ (→ MP4) |

## 🤖 Smart Features

### Auto Codec Detection
The app automatically analyzes:
- Video codec (H264, HEVC, SVQ3, etc)
- Audio codec (AAC, MP3, PCM, etc)
- Total streams & metadata
- Optimal encoding strategy

### 2-Step Conversion for Unsupported Formats
1. **Step 1:** Re-encode to H264 (preserves audio)
2. **Step 2:** Add thumbnail (clean output)
3. **Cleanup:** Remove temporary files

## 🐛 Troubleshooting

### ❌ "FFmpeg not found"
→ Install FFmpeg (see System Requirements) and add to PATH

### ❌ "File not found after reencode"
→ Check log message for full path  
→ Verify parent folder has write permissions

### ❌ "Black frames" in extraction
→ Take a screenshot instead (more reliable)  
→ Try different frame positions

### ❌ Thumbnail doesn't appear in Explorer
→ Click "Refresh Cache" button  
→ Or restart Windows Explorer

## 📁 Project Structure

```
video-thumbnail-changer/
├── video_thumbnail_changer_v4.9.py   # Main application
├── requirements.txt                   # Python dependencies
├── README.md                          # This file
└── screenshot.png                     # App preview
```

## 📦 Dependencies

```
pillow>=9.0.0
python-vlc>=3.0.0
```

## 🎯 Roadmap

- [ ] Batch processing (multiple videos)
- [ ] Text presets/templates
- [ ] Drag & drop support
- [ ] Audio waveform visualization
- [ ] Custom watermark overlay
- [ ] GPU acceleration profiles

## 📜 License

MIT License - Free to use and modify

## 🤝 Contributing

Issues and feature requests are welcome! 🎉

---

**Made with ❤️ for video creators** 🎬

⭐ If you find this useful, please star the repo!
