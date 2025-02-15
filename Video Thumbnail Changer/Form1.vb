Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports SharpCompress.Archives
Imports SharpCompress.Common

Public Class Form1
    ' Constants for Shell API
    Private Const SHGFI_ICON As Integer = &H100
    Private Const SHGFI_LARGEICON As Integer = &H0
    Private Const SHGFI_SMALLICON As Integer = &H1
    Private Const SHGFI_USEFILEATTRIBUTES As Integer = &H10
    Private Const SHGFI_SYSICONINDEX As Integer = &H4000
    Private Const SHGFI_TYPENAME As Integer = &H400

    ' Variables for handling video and thumbnails
    Private explorerHandle As IntPtr = IntPtr.Zero
    Private selectedVideo As String = ""
    Private thumbnails As New Dictionary(Of Integer, Image)
    Private selecting As Boolean = False
    Private startPoint As Point
    Private overlayRectangle As Rectangle
    Private isSelecting As Boolean = False
    Private selectionStartPoint As Point
    Private IconInThumbnail As Boolean = False ' Default value
    Private isCapturing As Boolean = False
    ' Dictionary to store backups of images
    Private BackupDict As New Dictionary(Of String, Image)
    ' Variable to store the path of the currently displayed image
    Private CurrentImageFilePath As String = ""

    Public Enum TextPosition
        UL ' Top left
        UM ' Top middle
        UR ' Top right
        BL ' Bottom left
        BM ' Bottom middle
        BR ' Bottom right
        MM ' Center screen
    End Enum

    Private currentTextPosition As TextPosition = TextPosition.BM

    ' Supported video extensions
    Public videoExtensions As String = "3g2;3gp;3gp2;3gpp;amv;asf;avi;bik;dds;divx;dpg;dv;dvr-ms;evo;f4v;flv;hdmov;k3g;m1v;m2t;m2ts;m2v;m4b;m4p;m4v;mk3d;mkv;mov;mp2v;mp4;mp4v;mpe;mpeg;mpg;mpv2;mpv4;mqv;mts;mxf;nsv;ogm;ogv;qt;ram;rm;rmvb;skm;swf;tp;tpr;trp;ts;vob;webm;wm;wmv;xvid"

    ' DLL Imports for Shell API
    <DllImport("shell32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
    Private Shared Function SHCreateItemFromParsingName(
        <MarshalAs(UnmanagedType.LPWStr)> pszPath As String,
        pbc As IntPtr,
        ByRef riid As Guid,
        <Out> ByRef ppv As IShellItemImageFactory) As Integer
    End Function

    <ComImport>
    <Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B")>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface IShellItemImageFactory
        Function GetImage(size As Size, flags As Integer, ByRef phbm As IntPtr) As Integer
    End Interface

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(hObject As IntPtr) As Boolean
    End Function

    Private Shared Function SHGetFileInfo(
        ByVal pszPath As String,
        ByVal dwFileAttributes As Integer,
        ByRef psfi As SHFILEINFO,
        ByVal cbFileInfo As Integer,
        ByVal uFlags As Integer) As IntPtr
    End Function

    Private Structure SHFILEINFO
        Public hIcon As IntPtr
        Public iIcon As Integer
        Public dwAttributes As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)>
        Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)>
        Public szTypeName As String
    End Structure

    ' INITIAL CONFIGURATION
    ' Form Load Event
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If File.Exists(backupPath) Then
            File.Delete(backupPath) ' Delete the old image on startup
        End If

        If My.Settings.ffmpegpath IsNot Nothing Then
            FFmpegPath.Text = My.Settings.ffmpegpath
        End If

        BGTextOverlay.BackColor = Color.Black
        FontTextOverlay.BackColor = Color.Yellow

        CustomOverlay.BackColor = BGTextOverlay.BackColor
        CustomOverlay.ForeColor = FontTextOverlay.BackColor

        Dim FFMPEGExe As String = GetFFMPEGPath()
        Dim icarosExe As String = GetIcarosPath()

        If File.Exists(FFMPEGExe) Then
            FFmpegPath.Text = FFMPEGExe
            DLFFmpegButton.Enabled = False
        Else
            DLFFmpegButton.Enabled = True
        End If

        If File.Exists(icarosExe) Then
            IcarosPathTextbox.Text = icarosExe
            DLIcarosButton.Enabled = False
        Else
            DLIcarosButton.Enabled = True
        End If

        ' Configure the ImageList for larger thumbnails
        ImageList1.ImageSize = New Size(120, 90) ' Increase thumbnail size
        ImageList1.ColorDepth = ColorDepth.Depth32Bit

        ' Add text position options
        TextPositionCombobox.Items.AddRange({"Up Left", "Up Middle", "Up Right", "Bottom Left", "Bottom Middle", "Bottom Right", "Screen Center"})
        TextPositionCombobox.SelectedIndex = 4 ' Default selection Bottom Middle

        ' Initialize the list and controls for multi-screen
        MergeMethodCombobox.Items.AddRange({"Alpha Blend", "Split Merge", "Mean Blend", "Grid Layout", "Layered Overlay"})
        MergeMethodCombobox.SelectedIndex = 3 ' Grid Layout

        UpdateStatusBox("Welcome to Video Thumbnail Changer!")
        FinalPictureBox.Image = Nothing
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        FolderPath.Focus()
    End Sub

    ' Get FFmpeg Path
    Private Function GetFFMPEGPath() As String
        If Not String.IsNullOrEmpty(FFmpegPath.Text) AndAlso File.Exists(FFmpegPath.Text) Then
            DLFFmpegButton.Enabled = False
            Return FFmpegPath.Text
        End If

        Dim ffmpegDirectory As String = Path.Combine(Application.StartupPath, "FFmpeg")
        If Directory.Exists(ffmpegDirectory) Then
            Dim ffmpegFiles As String() = Directory.GetFiles(ffmpegDirectory, "ffmpeg.exe", SearchOption.AllDirectories)
            If ffmpegFiles.Length > 0 Then
                FFmpegPath.Text = ffmpegFiles(0)
                DLFFmpegButton.Enabled = False
                Return ffmpegFiles(0)
            End If
        End If

        DLFFmpegButton.Enabled = True
        Return ""
    End Function

    Private Sub FFmpegPath_TextChanged(sender As Object, e As EventArgs) Handles FFmpegPath.TextChanged
        DLFFmpegButton.Enabled = Not File.Exists(FFmpegPath.Text)
    End Sub

    ' Get Icaros Path
    Private Function GetIcarosPath() As String
        If Not String.IsNullOrEmpty(IcarosPathTextbox.Text) AndAlso File.Exists(IcarosPathTextbox.Text) Then
            Return IcarosPathTextbox.Text
        End If

        Dim defaultPath As String = Path.Combine(Application.StartupPath, "Icaros\IcarosConfig.exe")
        If File.Exists(defaultPath) Then
            Return defaultPath
        End If

        Return ""
    End Function

    ' Download FFmpeg Button Click Event
    Private Async Sub DLFFmpegButton_Click(sender As Object, e As EventArgs) Handles DLFFmpegButton.Click
        Dim ffmpegUrl As String = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
        Dim zipPath As String = Path.Combine(Application.StartupPath, "FFmpeg.zip")
        Dim extractPath As String = Path.Combine(Application.StartupPath, "FFmpeg")

        Try
            Using httpClient As New HttpClient()
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0")

                Dim zipData As Byte() = Await httpClient.GetByteArrayAsync(ffmpegUrl)
                File.WriteAllBytes(zipPath, zipData)

                UpdateStatusBox("Download complete! Extracting...")

                If Not Directory.Exists(extractPath) Then Directory.CreateDirectory(extractPath)

                Using archive = ArchiveFactory.Open(zipPath)
                    archive.ExtractToDirectory(extractPath)
                End Using

                File.Delete(zipPath)

                ' Find FFmpeg.exe in the extracted folder
                Dim ffmpegExe As String = Directory.GetFiles(extractPath, "ffmpeg.exe", SearchOption.AllDirectories).FirstOrDefault()

                If Not String.IsNullOrEmpty(ffmpegExe) Then
                    FFmpegPath.Text = ffmpegExe
                    My.Settings.ffmpegpath = ffmpegExe
                    My.Settings.Save()
                    DLFFmpegButton.Enabled = False
                    UpdateStatusBox("FFmpeg downloaded and extracted successfully.")
                Else
                    UpdateStatusBox("Error: ffmpeg.exe not found after extraction.")
                End If
            End Using
        Catch ex As Exception
            UpdateStatusBox("Error downloading FFmpeg: " & ex.Message)
        End Try
    End Sub

    ' Download Icaros Button Click Event
    Private Async Sub DownloadIcarosButton_Click(sender As Object, e As EventArgs) Handles DLIcarosButton.Click
        Dim apiUrl As String = "https://api.github.com/repos/Xanashi/Icaros/releases/latest"
        Dim zipPath As String = Path.Combine(Application.StartupPath, "Icaros.zip")
        Dim extractPath As String = Path.Combine(Application.StartupPath, "Icaros")

        Try
            Using httpClient As New HttpClient()
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0")

                Dim json As String = Await httpClient.GetStringAsync(apiUrl)
                Dim zipUrl As String = ExtractZipUrlFromJson(json)
                If String.IsNullOrEmpty(zipUrl) Then
                    UpdateStatusBox("Unable to determine the ZIP file URL for the latest version.")
                    Return
                End If

                Dim zipData As Byte() = Await httpClient.GetByteArrayAsync(zipUrl)
                File.WriteAllBytes(zipPath, zipData)

                UpdateStatusBox("Download complete! Extracting...")

                If Not Directory.Exists(extractPath) Then Directory.CreateDirectory(extractPath)

                Using archive = ArchiveFactory.Open(zipPath)
                    archive.ExtractToDirectory(extractPath)
                End Using

                File.Delete(zipPath)

                Dim icarosExe As String = Path.Combine(extractPath, "IcarosConfig.exe")
                If File.Exists(icarosExe) Then
                    IcarosPathTextbox.Text = icarosExe
                    DLIcarosButton.Enabled = False
                    ConfigureAndLaunchIcaros()
                Else
                    UpdateStatusBox("Error: IcarosConfig.exe not found after extraction.")
                End If
            End Using
        Catch ex As Exception
            UpdateStatusBox("Error downloading Icaros: " & ex.Message)
        End Try
    End Sub

    ' Extract Zip URL from JSON
    Private Function ExtractZipUrlFromJson(json As String) As String
        Try
            Dim release As JObject = JObject.Parse(json)
            Dim assets As JArray = release("assets")

            For Each asset As JObject In assets
                Dim name As String = asset("name").ToString()
                If name.EndsWith(".zip") AndAlso Not name.Contains("Source", StringComparison.OrdinalIgnoreCase) Then
                    Return asset("browser_download_url").ToString()
                End If
            Next
        Catch ex As Exception
            UpdateStatusBox("Error extracting ZIP URL: " & ex.Message)
        End Try
        Return Nothing
    End Function

    ' Browse FFmpeg Button Click Event
    Private Sub BrowseFFmpeg_Click(sender As Object, e As EventArgs) Handles BrowseFFmpeg.Click
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe"
            openFileDialog.Title = "Select FFmpeg.exe"

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                FFmpegPath.Text = openFileDialog.FileName
                My.Settings.ffmpegpath = openFileDialog.FileName
                My.Settings.Save()
            End If
        End Using
    End Sub

    ' Browse Icaros Button Click Event
    Private Sub BrowseIcarosButton_Click(sender As Object, e As EventArgs) Handles BrowseIcarosButton.Click
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "IcarosConfig.exe|IcarosConfig.exe"
            openFileDialog.Title = "Select IcarosConfig.exe"

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                IcarosPathTextbox.Text = openFileDialog.FileName
                DLIcarosButton.Enabled = False
            End If
        End Using
    End Sub

    ' Configure and Launch Icaros
    Private Sub ConfigureAndLaunchIcaros()
        Dim icarosExe As String = Path.Combine(Application.StartupPath, "Icaros", "IcarosConfig.exe")
        ' Check if the user has administrator privileges
        If Not IsUserAnAdmin() Then
            UpdateStatusBox("You need administrator rights to configure Icaros.")
            Return
        End If

        For Each proc As Process In Process.GetProcessesByName("IcarosConfig")
            Try
                proc.Kill()
                proc.WaitForExit()
            Catch ex As Exception
                UpdateStatusBox("Unable to close IcarosConfig.exe: " & ex.Message)
            End Try
        Next
        If File.Exists(icarosExe) Then
            Try
                ConfigureIcarosRegistry()
                Dim process As New Process()
                process.StartInfo.FileName = icarosExe
                process.StartInfo.Arguments = "/Register"
                process.StartInfo.UseShellExecute = True
                process.StartInfo.Verb = "runas"
                process.Start()
                process.WaitForExit()

                ClearThumbnailCache()
            Catch ex As Exception
                UpdateStatusBox("Failed to launch Icaros. Ensure you have administrator rights.")
            End Try
        Else
            UpdateStatusBox("Error: IcarosConfig.exe not found.")
        End If
    End Sub

    ' Admin Detection
    Private Function IsUserAnAdmin() As Boolean
        Dim identity As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
        Dim principal As New System.Security.Principal.WindowsPrincipal(identity)
        Return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator)
    End Function

    ' Configure Icaros Registry
    Private Sub ConfigureIcarosRegistry()
        Try
            Dim icarosRegPath As String = "HKEY_CURRENT_USER\Software\Icaros"
            Dim icarosLocRegPath As String = "HKEY_CURRENT_USER\Software\Icaros\Cache"
            Dim fullPathLocation As String = Path.Combine(Application.StartupPath, "Icaros", "IcarosCache")

            If Not Directory.Exists(fullPathLocation) Then
                Directory.CreateDirectory(fullPathLocation)
            End If

            Microsoft.Win32.Registry.SetValue(icarosLocRegPath, "Location", fullPathLocation, Microsoft.Win32.RegistryValueKind.String)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "Cache", 2, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "UseEmbeddedImageAsCover", 1, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "ThumbnailingActive", 1, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "ThumbnailOffset", 25, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "DisplayIconOverlays", 1, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "DisplayMovieReel", 0, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "PreferLandscapeCovers", 1, Microsoft.Win32.RegistryValueKind.DWord)
            Microsoft.Win32.Registry.SetValue(icarosRegPath, "EnableBlackWhiteFilter", 1, Microsoft.Win32.RegistryValueKind.DWord)
        Catch ex As Exception
            UpdateStatusBox("Error configuring Icaros in the registry: " & ex.Message)
        End Try
    End Sub

    Private Sub ReleasePictureBoxes()
        Dim pictureBoxes As PictureBox() = {
            PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5,
            PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10, WindowsIconThumbnail, FinalPictureBox
        }

        For Each pb As PictureBox In pictureBoxes
            If pb.Image IsNot Nothing Then
                Try
                    ' Create a copy to detach from the file
                    Dim tempImage As New Bitmap(pb.Image)
                    pb.Image.Dispose() ' Release the resource
                    pb.Image = Nothing
                    pb.Image = tempImage ' Reassign a copy
                Catch ex As Exception
                    UpdateStatusBox("Error releasing a PictureBox: " & ex.Message)
                End Try
            End If
        Next

        ' Release memory
        GC.Collect()
        GC.WaitForPendingFinalizers()
    End Sub

    Private Sub EmptyMultiScreenStuff()
        ' Remove all images in ThumbnailFlowPanel
        Dim pictureBoxesToRemove As New List(Of PictureBox)()

        For Each control As Control In ThumbnailFlowPanel.Controls
            If TypeOf control Is PictureBox Then
                Dim pb As PictureBox = CType(control, PictureBox)
                If pb IsNot Nothing AndAlso pb.Tag IsNot Nothing Then
                    Dim filePath As String = pb.Tag.ToString()
                    If File.Exists(filePath) Then
                        Try
                            File.Delete(filePath)
                            UpdateStatusBox("✅ Deleted: " & filePath)
                        Catch ex As Exception
                            UpdateStatusBox("❌ Error deleting: " & ex.Message)
                        End Try
                    End If
                End If
                pictureBoxesToRemove.Add(pb)
            End If
        Next

        ' Remove PictureBoxes from FlowLayoutPanel
        For Each pb In pictureBoxesToRemove
            ThumbnailFlowPanel.Controls.Remove(pb)
            pb.Dispose()
        Next

        ' Clear the FlowLayoutPanel completely
        ThumbnailFlowPanel.Controls.Clear()

        ' Remove the merged image
        FinalPictureBox.Image = Nothing

        ' Also remove the merged backup if it exists
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If File.Exists(backupPath) Then
            Try
                File.Delete(backupPath)
                UpdateStatusBox("✅ Backup deleted")
            Catch ex As Exception
                UpdateStatusBox("❌ Error deleting backup: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub DeleteTemporaryScreenshots()
        Try
            Dim tempPath As String = Path.GetTempPath()
            Dim tempFiles As String() = Directory.GetFiles(tempPath, "thumb*.jpg")

            ' Remove images from PictureBoxes without clearing them
            ReleasePictureBoxes()

            ' Force memory release before deleting files
            GC.Collect()
            GC.WaitForPendingFinalizers()

            ' Delete temporary files
            For Each file As String In tempFiles
                If IO.File.Exists(file) Then
                    Try
                        IO.File.Delete(file)
                    Catch ex As Exception
                        UpdateStatusBox("Error deleting " & file & ": " & ex.Message)
                    End Try
                End If
            Next

        Catch ex As Exception
            UpdateStatusBox("Error deleting temporary screenshots: " & ex.Message)
        End Try
    End Sub

    ' Folder Path Text Changed Event
    Private Sub FolderPath_TextChanged(sender As Object, e As EventArgs) Handles FolderPath.TextChanged
        If Directory.Exists(FolderPath.Text) Then
            LoadVideos(FolderPath.Text)
        End If
    End Sub

    ' Select Folder Button Click Event
    Private Sub btnSelectFolder_Click(sender As Object, e As EventArgs) Handles btnSelectFolder.Click
        Using folderDialog As New FolderBrowserDialog()
            If folderDialog.ShowDialog() = DialogResult.OK Then
                FolderPath.Text = folderDialog.SelectedPath
                LoadVideos(folderDialog.SelectedPath)
            End If
        End Using
    End Sub

    ' LOADING VIDEOS
    ' Load Videos from Folder
    Private Sub LoadVideos(folderPath As String)
        VideoGrid.Controls.Clear()
        VideoFlowPanel.AutoScroll = True
        VideoGrid.FlowDirection = FlowDirection.LeftToRight
        VideoGrid.WrapContents = True
        VideoGrid.AutoScroll = True

        Dim extensions As String() = videoExtensions.Split(";"c).Select(Function(ext) "*." & ext).ToArray()
        Dim files As New List(Of String)

        For Each ext As String In extensions
            files.AddRange(Directory.GetFiles(folderPath, ext, SearchOption.TopDirectoryOnly))
        Next

        Dim index As Integer = 0
        For Each file As String In files
            ' Create the panel containing the video
            Dim videoPanel As New Panel With {
            .Size = New Size(130, 140),
            .Padding = New Padding(5),
            .BorderStyle = BorderStyle.FixedSingle ' Add black border around the panel
        }

            ' Create the video thumbnail
            Dim pb As New PictureBox With {
            .Width = 120,
            .Height = 80,
            .SizeMode = PictureBoxSizeMode.StretchImage,
            .Tag = file,
            .BorderStyle = BorderStyle.FixedSingle ' Add black border around the image
        }

            ' Load the Windows or FFmpeg thumbnail
            Dim thumbnail As Image = GetWindowsVideoThumbnail(file)
            If thumbnail IsNot Nothing Then
                pb.Image = AddBorderToImage(thumbnail, 5) ' Add a larger 5px black border
            End If

            ' Create the text below the thumbnail
            Dim lbl As New Label With {
            .Text = Path.GetFileNameWithoutExtension(file),
            .AutoSize = False,
            .Width = 120,
            .Height = 40,
            .TextAlign = ContentAlignment.MiddleCenter
        }

            ' Add a tooltip (info-bubble)
            Dim tooltip As New ToolTip()
            Dim fileInfo As New FileInfo(file)
            tooltip.SetToolTip(pb, $"Name: {fileInfo.Name}{vbCrLf}Size: {fileInfo.Length \ 1024} KB{vbCrLf}Date: {fileInfo.CreationTime}")

            ' Associate the click event
            AddHandler pb.Click, AddressOf VideoClicked

            ' Add elements to the video panel
            videoPanel.Controls.Add(pb)
            videoPanel.Controls.Add(lbl)

            ' Add the video panel to the video grid
            VideoGrid.Controls.Add(videoPanel)

            index += 1
        Next
    End Sub

    ' Get Windows Video Thumbnail
    Private Function GetWindowsVideoThumbnail(videoPath As String) As Image
        Try
            Dim factory As IShellItemImageFactory = Nothing
            Dim riid As Guid = GetType(IShellItemImageFactory).GUID
            Dim hr As Integer = SHCreateItemFromParsingName(videoPath, IntPtr.Zero, riid, factory)

            If hr = 0 AndAlso factory IsNot Nothing Then
                Dim hbitmap As IntPtr
                hr = factory.GetImage(New Size(256, 256), 0, hbitmap)

                If hr = 0 AndAlso hbitmap <> IntPtr.Zero Then
                    Dim img As Image = Image.FromHbitmap(hbitmap)
                    DeleteObject(hbitmap)
                    Return img
                End If
            End If
        Catch ex As Exception
            UpdateStatusBox("Error retrieving Windows thumbnail: " & ex.Message)
        End Try
        Return Nothing
    End Function

    ' Get FFmpeg Video Thumbnail
    Private Function GetFFmpegVideoThumbnail(videoPath As String, timestamp As Integer, index As Integer) As Image
        Dim tempThumbnail As String = Path.Combine(Path.GetTempPath(), $"thumb{index}.jpg")
        Dim ffmpegPath As String = Me.FFmpegPath.Text

        ' Check for FFmpeg
        If Not File.Exists(ffmpegPath) Then
            UpdateStatusBox("FFmpeg not found! Check the path.")
            Return Nothing
        End If

        ' Delete the file if it exists before generating a new screenshot
        If File.Exists(tempThumbnail) Then
            Try
                File.Delete(tempThumbnail)
            Catch ex As Exception
                UpdateStatusBox("Unable to delete " & tempThumbnail & ": " & ex.Message)
                Return Nothing
            End Try
        End If

        ' Execute the FFmpeg command
        Dim process As New Process()
        process.StartInfo.FileName = ffmpegPath
        process.StartInfo.Arguments = $"-threads 4 -ss {timestamp} -i ""{videoPath}"" -f image2 -an -frames:v 1 -q:v 2 -y ""{tempThumbnail}"""
        process.StartInfo.CreateNoWindow = True
        process.StartInfo.UseShellExecute = False
        process.Start()
        process.WaitForExit()

        ' Check if the file is created and load it properly
        If File.Exists(tempThumbnail) Then
            Try
                Using tempImg As Bitmap = New Bitmap(tempThumbnail)
                    Return New Bitmap(tempImg) ' Create a copy to avoid locking
                End Using
            Catch ex As Exception
                UpdateStatusBox("Error loading " & tempThumbnail & ": " & ex.Message)
                Return Nothing
            End Try
        Else
            UpdateStatusBox("FFmpeg did not generate the image.")
            Return Nothing
        End If
    End Function

    Private Function CheckThumbnailSupport(filePath As String) As Boolean
        Dim supportedFormats As String() = {".mp4", ".mkv", ".avi"}
        Return supportedFormats.Contains(Path.GetExtension(filePath).ToLower())
    End Function

    ' Load Thumbnails for Video
    Private Sub LoadThumbnails(videoPath As String)
        DeleteTemporaryScreenshots()
        Dim totalThumbnails As Integer = 10
        Dim videoDuration As Integer = GetVideoDuration(videoPath)
        If videoDuration = 0 Then Exit Sub

        Dim interval As Integer = videoDuration \ totalThumbnails
        Dim pictureBoxes As PictureBox() = {PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5,
                                        PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10}

        Dim tooltip As New ToolTip()

        For i As Integer = 0 To totalThumbnails - 1
            Dim timestamp As Integer = i * interval
            pictureBoxes(i).Tag = timestamp
            AddHandler pictureBoxes(i).Click, AddressOf Thumbnail_Clicked

            Dim thumbnail As Image = GetFFmpegVideoThumbnail(videoPath, timestamp, i + 1)
            If thumbnail IsNot Nothing Then
                Dim imageWithBorder As Image = AddBorderToImage(thumbnail, 5) ' Add a black border
                pictureBoxes(i).Image = imageWithBorder
                tooltip.SetToolTip(pictureBoxes(i), $"Capture at {Math.Round((timestamp / videoDuration) * 100)}%")
            End If
        Next
    End Sub

    Private Function GetEstimatedWindowsThumbnailTimeCode(videoDuration As Integer) As Integer
        ' Use a constant factor of 0.25
        Dim factor As Double = 0.25

        ' Calculate the timecode
        Dim estimatedTime As Integer = Math.Max(0, Math.Min(CInt(videoDuration * factor), videoDuration - 1))

        ' Debug: Display in the StatusBox
        UpdateStatusBox($"Video duration: {videoDuration} sec | Estimated Windows thumbnail timecode: {estimatedTime} sec")

        Return estimatedTime
    End Function

    ' Get Video Duration
    Private Function GetVideoDuration(videoPath As String) As Integer
        Dim ffmpegPath As String = Me.FFmpegPath.Text
        Dim output As String = ""

        If Not File.Exists(ffmpegPath) Then Return 0

        Dim process As New Process()
        process.StartInfo.FileName = ffmpegPath
        process.StartInfo.Arguments = $"-i ""{videoPath}"" 2>&1"
        process.StartInfo.RedirectStandardError = True
        process.StartInfo.UseShellExecute = False
        process.StartInfo.CreateNoWindow = True
        process.Start()

        output = process.StandardError.ReadToEnd()
        process.WaitForExit()

        Dim match = System.Text.RegularExpressions.Regex.Match(output, "Duration: (\d+):(\d+):(\d+)\.(\d+)")
        If match.Success Then
            Dim hours As Integer = Integer.Parse(match.Groups(1).Value)
            Dim minutes As Integer = Integer.Parse(match.Groups(2).Value)
            Dim seconds As Integer = Integer.Parse(match.Groups(3).Value)
            Return (hours * 3600) + (minutes * 60) + seconds
        End If

        Return 0
    End Function

    ' THUMBNAIL VIDEO PLAYER EVENT
    ' Play Video
    Private Sub PlayVideo()
        Video.Ctlcontrols.play()
    End Sub

    ' Pause Video
    Private Sub PauseVideo()
        Video.Ctlcontrols.pause()
    End Sub

    ' Stop Video
    Private Sub StopVideo()
        Video.Ctlcontrols.stop()
    End Sub

    ' Video Play State Changed Event
    Private Sub Video_PlayStateChanged(ByVal sender As Object, ByVal e As AxWMPLib._WMPOCXEvents_PlayStateChangeEvent)
        If e.newState = 3 Then
            Dim duration As Integer = Video.currentMedia.duration
        End If
    End Sub

    ' Original Windows Thumbnail click
    Private Sub WindowsThumbnail_Click(sender As Object, e As EventArgs) Handles WindowsIconThumbnail.Click
        If Video.currentMedia IsNot Nothing Then
            ' Check if the tag exists
            If WindowsIconThumbnail.Tag IsNot Nothing AndAlso IsNumeric(WindowsIconThumbnail.Tag) Then
                Dim position As Integer = CInt(WindowsIconThumbnail.Tag)
                Video.Ctlcontrols.currentPosition = position
                Video.Ctlcontrols.play()
            Else
                UpdateStatusBox("Error: Windows thumbnail timecode not found.")
            End If
        End If
    End Sub

    ' MERGE FUNCTIONS
    Private Function MergeImages() As Image
        Dim images As New List(Of Bitmap)
        Try
            ' Retrieve the size of the first screenshot to standardize the merge
            Dim baseSize As Size = Image.FromFile(CType(ThumbnailFlowPanel.Controls(0), PictureBox).Tag.ToString()).Size

            ' Load images into memory, resizing them to the correct size
            For Each control As Control In ThumbnailFlowPanel.Controls
                Dim pb As PictureBox = TryCast(control, PictureBox)
                If pb IsNot Nothing AndAlso File.Exists(pb.Tag.ToString()) Then
                    Dim img As New Bitmap(pb.Tag.ToString())
                    images.Add(ResizeImageToSize(img, baseSize))
                    img.Dispose()
                End If
            Next

            If images.Count < 2 Then
                UpdateStatusBox("Error loading images.")
                Return Nothing
            End If

            ' Retrieve the selected method
            Dim method As String = MergeMethodCombobox.SelectedItem.ToString()
            Dim resultImage As Bitmap = Nothing

            Select Case method
                Case "Alpha Blend"
                    resultImage = AlphaBlendMerge(images)
                Case "Split Merge"
                    resultImage = SplitMerge(images)
                Case "Mean Blend"
                    resultImage = MeanBlendMerge(images)
                Case "Grid Layout"
                    resultImage = GridMerge(images)
                Case "Layered Overlay"
                    resultImage = LayeredOverlayMerge(images)
            End Select

            ' Clean up memory
            For Each img In images
                img.Dispose()
            Next

            Return resultImage
        Catch ex As Exception
            UpdateStatusBox("Error merging: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Function AlphaBlendMerge(images As List(Of Bitmap)) As Bitmap
        Dim width As Integer = images(0).Width
        Dim height As Integer = images(0).Height
        Dim result As New Bitmap(width, height)

        Using g As Graphics = Graphics.FromImage(result)
            Dim alphaStep As Single = 1.0F / images.Count
            Dim currentAlpha As Single = 1.0F

            For Each img In images
                Dim cm As New Imaging.ColorMatrix With {
                .Matrix33 = currentAlpha ' Alpha of the image
            }
                Dim ia As New Imaging.ImageAttributes()
                ia.SetColorMatrix(cm, Imaging.ColorMatrixFlag.Default, Imaging.ColorAdjustType.Bitmap)

                g.DrawImage(img, New Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, ia)
                currentAlpha -= alphaStep
            Next
        End Using

        Return result
    End Function

    Private Function SplitMerge(images As List(Of Bitmap)) As Bitmap
        Dim width As Integer = images(0).Width
        Dim height As Integer = images(0).Height
        Dim result As New Bitmap(width, height)

        Using g As Graphics = Graphics.FromImage(result)
            Dim sliceHeight As Integer = height \ images.Count
            Dim yOffset As Integer = 0

            For Each img In images
                g.DrawImage(img, New Rectangle(0, yOffset, width, sliceHeight), New Rectangle(0, yOffset, width, sliceHeight), GraphicsUnit.Pixel)
                yOffset += sliceHeight
            Next
        End Using

        Return result
    End Function

    Private Function MeanBlendMerge(images As List(Of Bitmap)) As Bitmap
        Dim width As Integer = images(0).Width
        Dim height As Integer = images(0).Height
        Dim result As New Bitmap(width, height)

        Using g As Graphics = Graphics.FromImage(result)
            For Each img In images
                g.DrawImage(img, New Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel)
            Next
        End Using

        Return result
    End Function

    Private Function GridMerge(images As List(Of Bitmap)) As Bitmap
        Dim gridSize As Integer = Math.Ceiling(Math.Sqrt(images.Count))
        Dim imgWidth As Integer = images(0).Width
        Dim imgHeight As Integer = images(0).Height
        Dim result As New Bitmap(gridSize * imgWidth, gridSize * imgHeight)

        Using g As Graphics = Graphics.FromImage(result)
            Dim x As Integer = 0, y As Integer = 0
            For Each img In images
                g.DrawImage(img, New Rectangle(x * imgWidth, y * imgHeight, imgWidth, imgHeight), 0, 0, imgWidth, imgHeight, GraphicsUnit.Pixel)
                x += 1
                If x >= gridSize Then
                    x = 0
                    y += 1
                End If
            Next
        End Using

        Return result
    End Function

    Private Function LayeredOverlayMerge(images As List(Of Bitmap)) As Bitmap
        Dim width As Integer = images(0).Width
        Dim height As Integer = images(0).Height
        Dim result As New Bitmap(width, height)

        Using g As Graphics = Graphics.FromImage(result)
            Dim yOffset As Integer = 0
            Dim stepSize As Integer = height \ images.Count

            For Each img In images
                g.DrawImage(img, New Rectangle(0, yOffset, width, stepSize), 0, 0, width, height, GraphicsUnit.Pixel)
                yOffset += stepSize
            Next
        End Using

        Return result
    End Function

    Private Function ResizeImage(originalImage As Image, newSize As Size) As Image
        Dim resizedBitmap As New Bitmap(newSize.Width, newSize.Height)
        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newSize.Width, newSize.Height)
        End Using
        Return resizedBitmap
    End Function

    Private Function ResizeImageToSize(originalImage As Bitmap, newSize As Size) As Bitmap
        Dim resizedBitmap As New Bitmap(newSize.Width, newSize.Height)
        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newSize.Width, newSize.Height)
        End Using
        Return resizedBitmap
    End Function

    ' Add JPG Overlay
    Private Function AddJPGOverlay(img As Image) As Image
        ' Check if the image is valid
        If img Is Nothing Then
            UpdateStatusBox("Error: Invalid source image.")
            Return Nothing
        End If

        ' Create a copy of the image to avoid modifying the original
        Dim bmp As Bitmap
        Try
            bmp = New Bitmap(img)
        Catch ex As Exception
            UpdateStatusBox("Error copying the image: " & ex.Message)
            Return Nothing
        End Try

        ' Save a copy as backup before any modification
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        Try
            bmp.Save(backupPath, Imaging.ImageFormat.Jpeg)
        Catch ex As Exception
            UpdateStatusBox("Unable to save the backup: " & ex.Message)
        End Try
        Return bmp
    End Function

    Private Function ReloadFromBackupAndApplyOverlay() As Image
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")

        If Not File.Exists(backupPath) Then
            UpdateStatusBox("❌ No backup found, restore impossible.")
            Return Nothing
        End If

        Try
            Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim backupImg As New Bitmap(tempStream)
                Return ApplyOverlayToImage(backupImg) ' Apply overlay after restoring
            End Using
        Catch ex As Exception
            UpdateStatusBox("Error loading backup: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Sub IconInThumbnail_CheckedChanged(sender As Object, e As EventArgs)
        IconInThumbnail = CType(sender, CheckBox).Checked
        UpdateOverlay()
    End Sub

    ' Full-screen display with a toggle button to show/hide the overlay
    Private Sub ShowFullSizeImage()
        Dim fullSizeForm As New Form With {
            .Text = "Editing FinalScreenShot",
            .Size = New Size(FinalPictureBox.Image.Width + 20, FinalPictureBox.Image.Height + 80),
            .StartPosition = FormStartPosition.CenterScreen
        }

        Dim pictureBox As New PictureBox With {
            .Image = FinalPictureBox.Image,
            .SizeMode = PictureBoxSizeMode.Zoom,
            .Dock = DockStyle.Fill
        }

        Dim toggleButton As New Button With {
            .Text = "Hide Overlay",
            .Dock = DockStyle.Bottom
        }

        AddHandler toggleButton.Click, Sub(sender, e)
                                           If toggleButton.Text = "Hide Overlay" Then
                                               toggleButton.Text = "Show Overlay"
                                               pictureBox.Image = LoadBackupImage()
                                           Else
                                               toggleButton.Text = "Hide Overlay"
                                               pictureBox.Image = FinalPictureBox.Image
                                           End If
                                       End Sub

        fullSizeForm.Controls.Add(toggleButton)
        fullSizeForm.Controls.Add(pictureBox)
        fullSizeForm.ShowDialog()
    End Sub

    Private Function LoadBackupImage() As Image
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If Not File.Exists(backupPath) Then Return Nothing

        Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Return New Bitmap(tempStream)
        End Using
    End Function

    Private Sub FullSizePictureBox_MouseDown(sender As Object, e As MouseEventArgs)
        isSelecting = True
        selectionStartPoint = e.Location
    End Sub

    Private Sub FullSizePictureBox_MouseMove(sender As Object, e As MouseEventArgs)
        If isSelecting Then
            overlayRectangle = New Rectangle(
                Math.Min(e.X, selectionStartPoint.X),
                Math.Min(e.Y, selectionStartPoint.Y),
                Math.Abs(e.X - selectionStartPoint.X),
                Math.Abs(e.Y - selectionStartPoint.Y)
            )
            CType(sender, PictureBox).Invalidate()
        End If
    End Sub

    Private Sub FullSizePictureBox_MouseUp(sender As Object, e As MouseEventArgs)
        isSelecting = False
        Dim pb As PictureBox = CType(sender, PictureBox)

        ' If the selected area is too small, do nothing
        If overlayRectangle.Width < 5 OrElse overlayRectangle.Height < 5 Then
            overlayRectangle = Rectangle.Empty
            pb.Invalidate()
            Return
        End If

        ' Apply the overlay directly
        ApplyOverlayToImage(pb.Image)

        ' Update the PictureBox
        pb.Invalidate()
    End Sub

    Private Sub FullSizePictureBox_Paint(sender As Object, e As PaintEventArgs)
        Dim pb As PictureBox = CType(sender, PictureBox)
        If overlayRectangle.Width > 0 AndAlso overlayRectangle.Height > 0 Then
            Using pen As New Pen(Color.Red, 2)
                e.Graphics.DrawRectangle(pen, overlayRectangle)
            End Using
        End If
    End Sub

    Private Function GetBestFitFont(g As Graphics, text As String, rect As Rectangle, maxFontSize As Single) As Font
        Dim fontSize As Single = maxFontSize
        Dim font As Font = New Font("System", fontSize, FontStyle.Bold)

        ' Reduce the font size if necessary to fit within the area
        While fontSize > 5
            Dim textSize As SizeF = g.MeasureString(text, font)
            If textSize.Width <= rect.Width AndAlso textSize.Height <= rect.Height Then
                Return font
            End If
            fontSize -= 1
            font = New Font("System", fontSize, FontStyle.Bold)
        End While

        Return font
    End Function

    Private Sub SaveSelectionSettings()
        If overlayRectangle.Width > 0 AndAlso overlayRectangle.Height > 0 Then
            My.Settings.SelectionCustomSize = $"{overlayRectangle.X},{overlayRectangle.Y},{overlayRectangle.Width},{overlayRectangle.Height}"
            My.Settings.Save()
        End If
    End Sub

    Private Sub LoadSelectionSettings()
        Dim savedRect As String = My.Settings.SelectionCustomSize
        If Not String.IsNullOrEmpty(savedRect) Then
            Dim parts As String() = savedRect.Split(","c)
            If parts.Length = 4 Then
                overlayRectangle = New Rectangle(CInt(parts(0)), CInt(parts(1)), CInt(parts(2)), CInt(parts(3)))
            End If
        Else
            overlayRectangle = Rectangle.Empty
        End If
    End Sub

    ' Apply to Video File Click Event
    Private Sub ApplyToVidFile_Click(sender As Object, e As EventArgs) Handles ApplyToVidFile.Click
        If String.IsNullOrEmpty(selectedVideo) OrElse FinalPictureBox.Image Is Nothing Then
            UpdateStatusBox("Select a video and a thumbnail before applying.")
            Return
        End If

        If ApplyThumbnailToFile(selectedVideo) Then
            UpdateStatusBox("Successfully added to " & Path.GetFileName(selectedVideo))
            Beep()
            RefreshExplorerThumbnail(selectedVideo)
        Else
            UpdateStatusBox("Error applying thumbnail.")
        End If
    End Sub

    ' Refresh Explorer Thumbnail
    Private Sub RefreshExplorerThumbnail(filePath As String)
        Try
            Dim shell As Object = CreateObject("Shell.Application")
            Dim folder As Object = shell.Namespace(Path.GetDirectoryName(filePath))
            Dim item As Object = folder.ParseName(Path.GetFileName(filePath))
            If item IsNot Nothing Then
                item.InvokeVerb("Refresh")
            End If
        Catch ex As Exception
            UpdateStatusBox("Unable to refresh thumbnail. Open the folder manually.")
        End Try
    End Sub

    Private Sub ClearThumbnailCache()
        Try
            ' Delete files in IcarosCache
            Dim icarosCachePath As String = Path.Combine(Application.StartupPath, "Icaros", "IcarosCache")
            If Directory.Exists(icarosCachePath) Then
                For Each file As String In Directory.GetFiles(icarosCachePath)
                    Try
                        System.IO.File.Delete(file)
                    Catch ex As Exception
                        ' Ignore deletion errors
                    End Try
                Next
            End If
        Catch ex As Exception
            UpdateStatusBox("Unable to delete thumbnail cache: " & ex.Message)
        End Try
    End Sub

    Private Sub SaveBackup()
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If FinalPictureBox.Image IsNot Nothing Then
            FinalPictureBox.Image.Save(backupPath, Imaging.ImageFormat.Jpeg)
        End If
    End Sub

    Private Function LoadBackupForScreenshot(screenshotPath As String) As Image
        Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
        Dim backupPath As String = Path.Combine(backupFolder, Path.GetFileName(screenshotPath))

        If File.Exists(backupPath) Then
            Return New Bitmap(backupPath)
        Else
            UpdateStatusBox("❌ No backup found for this image.")
            Return Nothing
        End If
    End Function

    Private Sub SaveBackupForMerge(mergedImage As Image)
        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            Dim mergeBackupPath As String = Path.Combine(backupFolder, "merged_image.jpg")
            mergedImage.Save(mergeBackupPath, Imaging.ImageFormat.Jpeg)
        Catch ex As Exception
            UpdateStatusBox("❌ Error saving the merge backup: " & ex.Message)
        End Try
    End Sub

    Private Sub MultiScreen_DoubleClick(sender As Object, e As EventArgs) Handles ThumbnailFlowPanel.DoubleClick
        If ThumbnailFlowPanel.Controls.Count > 0 Then
            Dim selectedItem As Control = ThumbnailFlowPanel.Controls(ThumbnailFlowPanel.Controls.Count - 1)
            Dim fileToDelete As String = selectedItem.Tag.ToString()

            If File.Exists(fileToDelete) Then
                Try
                    File.Delete(fileToDelete)
                    UpdateStatusBox("🗑️ Screenshot deleted: " & fileToDelete)
                Catch ex As Exception
                    UpdateStatusBox("❌ Error deleting: " & ex.Message)
                End Try
            End If

            ThumbnailFlowPanel.Controls.Remove(selectedItem)
            UpdateMergeControlsVisibility()
        End If
    End Sub

    Private Sub ResetScreenshotState()
        Threading.Thread.Sleep(100)
        TakeScreenShot.Enabled = True
        isCapturing = False
    End Sub

    Private Sub SaveBackupForScreenshot(img As Image)
        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            Dim backupPath As String = Path.Combine(backupFolder, "last_screenshot.jpg")

            ' Verify and delete the existing file before saving
            If File.Exists(backupPath) Then
                File.Delete(backupPath)
            End If

            img.Save(backupPath, Imaging.ImageFormat.Jpeg)
            UpdateStatusBox("✅ Backup saved: " & backupPath)
        Catch ex As Exception
            UpdateStatusBox("❌ Error saving backup: " & ex.Message)
        End Try
    End Sub

    Private Sub SaveBackupForFinalPictureBox()
        If FinalPictureBox.Image Is Nothing Then Return

        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            Dim backupPath As String = Path.Combine(backupFolder, "final_screenshot_backup.jpg")

            ' Delete the old backup if it exists
            If File.Exists(backupPath) Then File.Delete(backupPath)

            FinalPictureBox.Image.Save(backupPath, Imaging.ImageFormat.Jpeg)
            UpdateStatusBox("✅ Image backup saved.")

        Catch ex As Exception
            UpdateStatusBox("❌ Error saving backup: " & ex.Message)
        End Try
    End Sub

    Private Sub UpdateOverlay()
        If FinalPictureBox.Image Is Nothing Then Return

        ' Save the image before modification
        SaveBackupForFinalPictureBox()

        ' Apply the overlay
        Dim updatedImg As Image = ApplyOverlayToImage(FinalPictureBox.Image)
        If updatedImg IsNot Nothing Then
            FinalPictureBox.Image = updatedImg
            FinalPictureBox.Refresh()
        End If
    End Sub

    Private Sub SaveOriginalScreenshot(img As Image, filePath As String)
        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            ' Save the original with a suffix _original.jpg
            Dim originalPath As String = Path.Combine(backupFolder, Path.GetFileNameWithoutExtension(filePath) & "_original.jpg")

            ' Verify and delete the existing file before saving
            If File.Exists(originalPath) Then File.Delete(originalPath)

            img.Save(originalPath, Imaging.ImageFormat.Jpeg)
            UpdateStatusBox("✅ Original screenshot saved without overlay: " & originalPath)

        Catch ex As Exception
            UpdateStatusBox("❌ Error saving original screenshot: " & ex.Message)
        End Try
    End Sub

    Private Sub FinalPictureBox_Click(sender As Object, e As EventArgs) Handles FinalPictureBox.Click
        If FinalPictureBox.Image Is Nothing Then Return

        ' Make a copy of the current image to avoid any modification
        Dim tempCopy As New Bitmap(FinalPictureBox.Image)

        ' Open a window to display the image in full size
        Dim fullSizeForm As New Form With {
        .Text = "Final Image Preview",
        .Size = New Size(tempCopy.Width + 20, tempCopy.Height + 80),
        .StartPosition = FormStartPosition.CenterScreen
    }

        Dim pictureBox As New PictureBox With {
        .Image = tempCopy,
        .SizeMode = PictureBoxSizeMode.Zoom,
        .Dock = DockStyle.Fill
    }

        fullSizeForm.Controls.Add(pictureBox)
        fullSizeForm.ShowDialog()

        ' Clean up memory
        pictureBox.Image.Dispose()
        tempCopy.Dispose()
    End Sub

    Private Sub AddThumbnailToFlowPanel(image As Image, filePath As String)
        Dim thumbnailSize As Integer = 80 ' Thumbnail size
        Dim pb As New PictureBox With {
        .Width = thumbnailSize,
        .Height = thumbnailSize,
        .SizeMode = PictureBoxSizeMode.Zoom,
        .Image = New Bitmap(image, thumbnailSize, thumbnailSize),
        .Tag = filePath ' Store the file path
    }

        ' Add an event to update FinalPictureBox when clicked
        AddHandler pb.Click, AddressOf Thumbnail_Clicked

        ' Add the image to the FlowLayoutPanel
        ThumbnailFlowPanel.Controls.Add(pb)

        UpdateMergeControlsVisibility()
    End Sub

    Private Function ResizeImageToMaxSize(originalImage As Bitmap, maxHeight As Integer) As Bitmap
        Dim newWidth As Integer = originalImage.Width
        Dim newHeight As Integer = originalImage.Height

        If newHeight > maxHeight Then
            Dim ratio As Double = maxHeight / CDbl(newHeight)
            newWidth = CInt(newWidth * ratio)
            newHeight = maxHeight
        End If

        Dim resizedBitmap As New Bitmap(newWidth, newHeight)
        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newWidth, newHeight)
        End Using

        Return resizedBitmap
    End Function

    Private Sub SaveBackupForScreenshot(screenshotPath As String)
        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            ' Create a backup file BEFORE overlay
            Dim originalBackupPath As String = Path.Combine(backupFolder, Path.GetFileNameWithoutExtension(screenshotPath) & "_original.jpg")

            ' Verify and delete if a backup already exists
            If File.Exists(originalBackupPath) Then File.Delete(originalBackupPath)

            ' Save the original image
            File.Copy(screenshotPath, originalBackupPath, True)

            UpdateStatusBox("✅ Backup saved: " & originalBackupPath)
        Catch ex As Exception
            UpdateStatusBox("❌ Error saving backup: " & ex.Message)
        End Try
    End Sub

    Private Sub MultiScreenThumbnail_CheckedChanged(sender As Object, e As EventArgs)
        ' Iterate through the child controls of the FlowLayoutPanel
        For Each control As Control In ThumbnailFlowPanel.Controls
            If TypeOf control Is PictureBox Then
                Dim pb As PictureBox = CType(control, PictureBox)
                Dim filePath As String = pb.Tag.ToString()
                If File.Exists(filePath) Then
                    Try
                        File.Delete(filePath)
                    Catch ex As Exception
                        UpdateStatusBox("Error deleting: " & ex.Message)
                    End Try
                End If
            End If
        Next

        ' Clear the FlowLayoutPanel
        ThumbnailFlowPanel.Controls.Clear()

        ' Reset the final image
        FinalPictureBox.Image = Nothing

        ' Update the visibility of merge controls
        UpdateMergeControlsVisibility()

        ' Restore the image from backup if available
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If File.Exists(backupPath) Then
            Try
                Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Dim originalImage As New Bitmap(tempStream)
                    FinalPictureBox.Image = ResizeImageToMaxSize(originalImage, 800)
                End Using
                UpdateStatusBox("Backup restored after disabling MultiScreen.")
            Catch ex As Exception
                UpdateStatusBox("Error loading backup: " & ex.Message)
            End Try
        Else
            UpdateStatusBox("No backup found, unable to restore the original image.")
        End If
    End Sub

    Private Sub BackupFinalImage()
        If FinalPictureBox.Image Is Nothing Then Return

        Try
            Dim backupFolder As String = Path.Combine(Path.GetTempPath(), "ScreenshotBackups")
            If Not Directory.Exists(backupFolder) Then Directory.CreateDirectory(backupFolder)

            Dim backupPath As String = Path.Combine(backupFolder, "final_backup.jpg")

            ' Verify and delete the existing backup before saving the new one
            If File.Exists(backupPath) Then File.Delete(backupPath)

            ' Save the displayed image before any modification
            FinalPictureBox.Image.Save(backupPath, Imaging.ImageFormat.Jpeg)

            UpdateStatusBox("✅ Backup saved for the displayed image.")
        Catch ex As Exception
            UpdateStatusBox("❌ Error saving backup: " & ex.Message)
        End Try
    End Sub

    Private Sub MultiScreen_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ThumbnailFlowPanel.ControlAdded
        If ThumbnailFlowPanel.Controls.Count = 0 Then Return

        Dim pb As PictureBox = TryCast(ThumbnailFlowPanel.Controls(ThumbnailFlowPanel.Controls.Count - 1), PictureBox)
        If pb IsNot Nothing AndAlso File.Exists(pb.Tag.ToString()) Then
            Using tempImg As New Bitmap(pb.Tag.ToString())
                FinalPictureBox.Image = New Bitmap(tempImg)
            End Using
            UpdateStatusBox("✅ Original image restored.")
        Else
            UpdateStatusBox("❌ Error: file not found.")
        End If
    End Sub

    Private Sub MergeScreen_Click(sender As Object, e As EventArgs) Handles MergeScreen.Click
        Dim mergedImg As Image = MergeImages()
        If mergedImg IsNot Nothing Then
            FinalPictureBox.Image = ResizeImageToMaxSize(mergedImg, 800)
            UpdateStatusBox("✅ Merge successful.")

            ' Create a backup of the merged image BEFORE overlay
            BackupFinalImage()
        Else
            UpdateStatusBox("❌ Error merging.")
        End If
    End Sub

    ' Inline assignment helper to avoid compilation errors
    Private Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function

    Private Function ResizeThumbnail(originalImage As Image, maxHeight As Integer) As Bitmap
        Dim ratio As Double = maxHeight / CDbl(originalImage.Height)
        Dim newWidth As Integer = CInt(originalImage.Width * ratio)
        Dim resizedBitmap As New Bitmap(newWidth, maxHeight)

        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newWidth, maxHeight)
        End Using

        Return resizedBitmap
    End Function

    Private Sub ApplyOverlayFromBackup()
        Try
            Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup_" & Path.GetFileNameWithoutExtension(selectedVideo) & ".jpg")

            If Not File.Exists(backupPath) Then
                UpdateStatusBox("❌ No backup found, overlay canceled.")
                Return
            End If

            ' Load the backup image
            Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim baseImage As New Bitmap(tempStream)

                ' Apply the overlay
                Dim overlayedImage As Image = ApplyOverlayToImage(baseImage)

                ' Update the display
                FinalPictureBox.Image = ResizeImageToMaxSize(overlayedImage, 800)
                FinalPictureBox.Refresh()
            End Using

            UpdateStatusBox("✅ Overlay applied successfully.")

        Catch ex As Exception
            UpdateStatusBox("❌ Error applying overlay: " & ex.Message)
        End Try
    End Sub

    Private Sub VideoClicked(sender As Object, e As EventArgs)
        ' Clear MultiScreen before loading a new video
        EmptyMultiScreenStuff()

        Dim pb As PictureBox = TryCast(sender, PictureBox)
        If pb?.Tag Is Nothing OrElse String.IsNullOrEmpty(pb.Tag.ToString()) Then
            UpdateStatusBox("❌ Error: no video associated with this thumbnail.")
            Return
        End If

        selectedVideo = pb.Tag.ToString()

        ' Verify if the file exists
        If Not File.Exists(selectedVideo) Then
            UpdateStatusBox("❌ The selected video does not exist.")
            Return
        End If

        ' Update information for the selected video
        ActualFileName.Text = Path.GetFileName(selectedVideo)
        GroupThumb.Text = $"Thumbnails from {ActualFileName.Text}"

        ' Retrieve the video duration and display it
        Dim videoDuration As Integer = GetVideoDuration(selectedVideo)
        If videoDuration > 0 Then
            Dim formattedDuration As String = TimeSpan.FromSeconds(videoDuration).ToString("hh\:mm\:ss")
            VideoFullTimeTextbox.Text = formattedDuration ' Update the TextBox
        Else
            VideoFullTimeTextbox.Text = "00:00:00" ' If duration is not found
        End If

        ' Check thumbnail compatibility
        Dim supportsThumbnails As Boolean = CheckThumbnailSupport(selectedVideo)

        ' Load the Windows thumbnail (always available)
        Dim windowsThumbnail As Image = GetWindowsVideoThumbnail(selectedVideo)
        WindowsIconThumbnail.Image = windowsThumbnail

        ' Manage interface display based on thumbnail support
        Dim thumbnailsVisible As Boolean = supportsThumbnails
        ThumbnailEdition.Visible = thumbnailsVisible
        VideoFileActionsGroup.Visible = thumbnailsVisible

        ' Load thumbnails only if the format is supported
        If supportsThumbnails Then LoadThumbnails(selectedVideo)

        ' Load the video into the player
        Try
            With Video
                .URL = selectedVideo
                .Ctlcontrols.stop()
                .uiMode = "full"
                .settings.volume = 50
                .fullScreen = False
                AddHandler .PlayStateChange, AddressOf Video_PlayStateChanged
                .Ctlcontrols.play()
            End With
        Catch ex As Exception
            UpdateStatusBox("❌ Error loading video: " & ex.Message)
        End Try
    End Sub

    Function AddBorderToImage(ByVal img As Image, ByVal borderWidth As Integer) As Image
        Dim newImg As New Bitmap(img.Width + (borderWidth * 2), img.Height + (borderWidth * 2))
        Using g As Graphics = Graphics.FromImage(newImg)
            g.Clear(Color.Black) ' Black border
            g.DrawImage(img, borderWidth, borderWidth, img.Width, img.Height) ' Center the image
        End Using
        Return newImg
    End Function

    Private Sub UpdateMergeControlsVisibility()
        Dim imageCount As Integer = ThumbnailFlowPanel.Controls.OfType(Of PictureBox)().Count()

        If imageCount >= 2 Then
            MergeScreen.Visible = True
            MergeMethodCombobox.Visible = True
        Else
            MergeScreen.Visible = False
            MergeMethodCombobox.Visible = False
        End If

        UpdateStatusBox("🖼️ Number of screenshots in FlowPanel: " & imageCount)
    End Sub

    Private Function ApplyOverlayToImage(originalImg As Image) As Image
        If originalImg Is Nothing Then Return Nothing

        Dim bmp As New Bitmap(originalImg)
        Using g As Graphics = Graphics.FromImage(bmp)
            Dim pos As Point
            Dim BordersSize As Integer = Math.Max(5, Math.Min(If(Integer.TryParse(MarginBorder.Text, BordersSize), BordersSize, 15), 100))
            Dim fontSize As Integer = Math.Max(5, Math.Min(If(Integer.TryParse(OverlaySize.Text, fontSize), fontSize, 40), 100))

            Dim font As Font = New Font("Arial", fontSize, FontStyle.Bold)
            Dim textSize As SizeF = g.MeasureString(CustomOverlay.Text, font)
            Dim rectWidth As Integer = CInt(textSize.Width) + 20
            Dim rectHeight As Integer = CInt(textSize.Height) + 20

            Select Case currentTextPosition
                Case TextPosition.UL : pos = New Point(BordersSize, BordersSize)
                Case TextPosition.UM : pos = New Point((bmp.Width - rectWidth) \ 2, BordersSize)
                Case TextPosition.UR : pos = New Point(bmp.Width - rectWidth - BordersSize, BordersSize)
                Case TextPosition.BL : pos = New Point(BordersSize, bmp.Height - rectHeight - BordersSize)
                Case TextPosition.BM : pos = New Point((bmp.Width - rectWidth) \ 2, bmp.Height - rectHeight - BordersSize)
                Case TextPosition.BR : pos = New Point(bmp.Width - rectWidth - BordersSize, bmp.Height - rectHeight - BordersSize)
                Case TextPosition.MM : pos = New Point((bmp.Width - rectWidth) \ 2, (bmp.Height - rectHeight) \ 2)
            End Select

            g.FillRectangle(New SolidBrush(BGTextOverlay.BackColor), New Rectangle(pos.X, pos.Y, rectWidth, rectHeight))
            g.DrawString(CustomOverlay.Text, font, New SolidBrush(FontTextOverlay.BackColor), pos.X + 10, pos.Y + 10)
        End Using

        Return bmp
    End Function

    Private Sub UpdateFinalImage()
        If FinalPictureBox.Image Is Nothing Then Return

        ' Save the image before modification
        SaveBackupForFinalPictureBox()

        ' Apply the overlay
        Dim updatedImg As Image = ApplyOverlayToImage(FinalPictureBox.Image)
        If updatedImg IsNot Nothing Then
            FinalPictureBox.Image = updatedImg
            FinalPictureBox.Refresh()
        End If
    End Sub

    Private Sub TakeScreenShot_Click(sender As Object, e As EventArgs) Handles TakeScreenShot.Click
        If isCapturing Then Exit Sub
        isCapturing = True
        TakeScreenShot.Enabled = False

        If String.IsNullOrEmpty(selectedVideo) Then
            UpdateStatusBox("❌ No video is currently playing.")
            ResetScreenshotState()
            Return
        End If

        Dim timestamp As Integer = CInt(Video.Ctlcontrols.currentPosition)
        Dim tempFolder As String = Path.Combine(Path.GetTempPath(), "VideoThumbnails")
        If Not Directory.Exists(tempFolder) Then Directory.CreateDirectory(tempFolder)

        Dim uniqueFilename As String = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg"
        Dim filePath As String = Path.Combine(tempFolder, uniqueFilename)

        ' Capture the screenshot
        Dim screenshot As Image = GetFFmpegVideoThumbnail(selectedVideo, timestamp, 99)
        If screenshot Is Nothing Then
            UpdateStatusBox("❌ Error: The captured image is empty or invalid.")
            ResetScreenshotState()
            Return
        End If

        Try
            screenshot.Save(filePath, Imaging.ImageFormat.Jpeg)
            BackupDict(filePath) = New Bitmap(screenshot) ' Save the original image

            ' Add to the FlowPanel
            AddThumbnailToFlowPanel(screenshot, filePath)

            ' Immediately load the image into the FinalPictureBox
            LoadImageInFinalPictureBox(filePath)

            UpdateStatusBox("✅ Screenshot added and saved.")
        Catch ex As Exception
            UpdateStatusBox("❌ Error: " & ex.Message)
        Finally
            ResetScreenshotState()
        End Try
    End Sub

    Private Function GetCurrentImageFilePath() As String
        For Each pb As PictureBox In ThumbnailFlowPanel.Controls
            If pb.Image IsNot Nothing AndAlso pb.Image.Equals(FinalPictureBox.Image) Then
                Return pb.Tag.ToString()
            End If
        Next
        Return ""
    End Function

    Private Sub Thumbnail_Clicked(sender As Object, e As EventArgs)
        Dim pb As PictureBox = CType(sender, PictureBox)
        If pb Is Nothing OrElse pb.Tag Is Nothing Then Exit Sub

        Debug.WriteLine("Thumbnail clicked: " & pb.Tag.ToString())

        Dim timestamp As Integer
        If Integer.TryParse(pb.Tag.ToString(), timestamp) Then
            If Video.currentMedia IsNot Nothing Then
                Video.Ctlcontrols.currentPosition = timestamp
                Video.Ctlcontrols.play()
                Debug.WriteLine("Jumped to: " & timestamp & " seconds")
            Else
                UpdateStatusBox("❌ No video is currently playing.")
            End If
        Else
            UpdateStatusBox("❌ Error: unable to convert the timestamp.")
        End If
    End Sub

    Private Sub LoadImageInFinalPictureBox(filePath As String)
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then Exit Sub

        Dim originalImage As Image
        If BackupDict.ContainsKey(filePath) Then
            originalImage = New Bitmap(BackupDict(filePath)) ' Load the original image from the backup
        Else
            originalImage = Image.FromFile(filePath) ' Load the image from disk
        End If

        ' Update the final PictureBox
        If FinalPictureBox.Image IsNot Nothing Then FinalPictureBox.Image.Dispose()
        FinalPictureBox.Image = originalImage

        ' Store the currently displayed file
        CurrentImageFilePath = filePath

        UpdateStatusBox("✅ Image displayed in FinalPictureBox.")
    End Sub

    Private Sub ResetOverlay_Click(sender As Object, e As EventArgs) Handles ResetOverlay.Click
        If String.IsNullOrEmpty(CurrentImageFilePath) Then
            UpdateStatusBox("❌ No file to reset.")
            Exit Sub
        End If

        ' Reload the original image without overlay
        LoadImageInFinalPictureBox(CurrentImageFilePath)

        UpdateStatusBox("🔄 Reset: reverted to the original image without overlay.")
    End Sub

    Private Sub ApplyOverlayFromGroupBox()
        If String.IsNullOrEmpty(CurrentImageFilePath) OrElse Not File.Exists(CurrentImageFilePath) Then Exit Sub

        ' Reset: Reload the original image
        LoadImageInFinalPictureBox(CurrentImageFilePath)

        ' Apply the overlay to this image
        Dim overlayedImage As Image = ApplyOverlayToImage(FinalPictureBox.Image)

        ' Update FinalPictureBox
        If overlayedImage IsNot Nothing Then
            FinalPictureBox.Image.Dispose()
            FinalPictureBox.Image = overlayedImage
            FinalPictureBox.Refresh()
            UpdateStatusBox("🎨 Overlay applied after reset.")
        End If
    End Sub

    Private Sub TextPositionCombobox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TextPositionCombobox.SelectedIndexChanged
        currentTextPosition = CType(TextPositionCombobox.SelectedIndex, TextPosition)
        ApplyOverlayFromGroupBox()
    End Sub

    Private Sub OverlaySize_TextChanged(sender As Object, e As EventArgs) Handles OverlaySize.TextChanged
        ApplyOverlayFromGroupBox()
    End Sub

    Private Sub MarginBorder_TextChanged(sender As Object, e As EventArgs) Handles MarginBorder.TextChanged
        ApplyOverlayFromGroupBox()
    End Sub

    Private Sub BGTextOverlay_Click(sender As Object, e As EventArgs) Handles BGTextOverlay.Click
        Using colorDialog As New ColorDialog()
            If colorDialog.ShowDialog() = DialogResult.OK Then
                BGTextOverlay.BackColor = colorDialog.Color
                ApplyOverlayFromGroupBox()
            End If
        End Using
    End Sub

    Private Sub FontTextOverlay_Click(sender As Object, e As EventArgs) Handles FontTextOverlay.Click
        Using colorDialog As New ColorDialog()
            If colorDialog.ShowDialog() = DialogResult.OK Then
                FontTextOverlay.BackColor = colorDialog.Color
                ApplyOverlayFromGroupBox()
            End If
        End Using
    End Sub

    Private Sub CustomOverlay_TextChanged(sender As Object, e As EventArgs) Handles CustomOverlay.TextChanged
        ApplyOverlayFromGroupBox()
    End Sub

    Private Function GetVideoInfo(videoPath As String) As String
        Try
            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then Return ""

            Dim output As String = ""

            Dim process As New Process()
            process.StartInfo.FileName = ffmpegPath
            process.StartInfo.Arguments = $"-i ""{videoPath}"" 2>&1"
            process.StartInfo.RedirectStandardError = True
            process.StartInfo.UseShellExecute = False
            process.StartInfo.CreateNoWindow = True
            process.Start()

            output = process.StandardError.ReadToEnd()
            process.WaitForExit()

            Return output
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Private Function IsFileReadable(filePath As String) As Boolean
        Try
            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then
                UpdateStatusBox("❌ FFmpeg not found.")
                Return False
            End If

            ' FFmpeg command: read 5 seconds of video without displaying output
            Dim cmd As String = $"-v error -i ""{filePath}"" -t 5 -f null -"

            ' Configure the FFmpeg process
            Dim process As New Process() With {
            .StartInfo = New ProcessStartInfo(ffmpegPath, cmd) With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardError = True
            }
        }

            process.Start()

            ' Read FFmpeg logs in real-time to detect errors
            Dim errorLog As String = ""
            Dim outputReader As StreamReader = process.StandardError
            Dim startTime As DateTime = DateTime.Now

            While Not process.HasExited
                ' Timeout to prevent hanging (e.g., corrupted file)
                If (DateTime.Now - startTime).TotalSeconds > 10 Then
                    process.Kill()
                    UpdateStatusBox("⏳ Timeout: FFmpeg is not responding, file may be corrupted.")
                    Return False
                End If

                Dim line As String = outputReader.ReadLine()
                If line IsNot Nothing Then
                    errorLog &= line & vbCrLf
                End If
            End While

            process.WaitForExit()

            ' If FFmpeg completes successfully, the file is OK
            If process.ExitCode = 0 Then
                UpdateStatusBox("✅ File is readable: " & filePath)
                Return True
            Else
                ' If an error is found, display the FFmpeg log
                UpdateStatusBox("⚠️ File is corrupted: " & filePath & vbCrLf & errorLog)
                Return False
            End If

        Catch ex As Exception
            UpdateStatusBox("❌ Error checking file: " & ex.Message)
            Return False
        End Try
    End Function

    ' Check if the video format supports thumbnails via FFmpeg
    Private Function IsThumbnailCompatible(filePath As String) As Boolean
        Try
            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then
                UpdateStatusBox("❌ FFmpeg not found.")
                Return False
            End If

            ' Command compatible with all FFmpeg versions
            Dim cmd As String = $"-i ""{filePath}"" -hide_banner"
            Dim process As New Process() With {
            .StartInfo = New ProcessStartInfo(ffmpegPath, cmd) With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardError = True,
                .StandardErrorEncoding = System.Text.Encoding.UTF8
            }
        }

            process.Start()
            Dim errorOutput As String = process.StandardError.ReadToEnd()
            process.WaitForExit()

            ' Search for the line containing 'major_brand' (indicates format)
            Dim formatLine As String = errorOutput.Split(vbLf).FirstOrDefault(Function(l) l.Contains("major_brand"))
            If String.IsNullOrEmpty(formatLine) Then
                UpdateStatusBox("⚠️ Unable to detect file format.")
                Return False
            End If

            ' Extract the format from 'major_brand'
            Dim detectedFormat As String = formatLine.Split(":"c).Last().Trim().ToLower()

            ' List of formats compatible with thumbnails
            Dim supportedFormats As String() = {"mp42", "isom", "mov", "mp41"}

            If supportedFormats.Contains(detectedFormat) Then
                UpdateStatusBox($"✅ Detected format: {detectedFormat} → Compatible with thumbnails.")
                Return True
            Else
                UpdateStatusBox($"❌ Detected format: {detectedFormat} → This format does NOT support thumbnails.")
                Return False
            End If
        Catch ex As Exception
            UpdateStatusBox("❌ Error checking format: " & ex.Message)
            Return False
        End Try
    End Function

    ' Apply a thumbnail to a video file
    Private Function ApplyThumbnailToFile(targetFile As String) As Boolean
        Try
            Dim tempThumbnailPath As String = Path.Combine(Path.GetTempPath(), "new_thumbnail.jpg")
            FinalPictureBox.Image.Save(tempThumbnailPath, Imaging.ImageFormat.Jpeg)

            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then
                UpdateStatusBox("❌ FFmpeg not found.")
                Return False
            End If

            ' Check format compatibility
            If Not IsThumbnailCompatible(targetFile) Then
                Dim result As DialogResult = MessageBox.Show("The format of this file is not compatible with thumbnails." & vbCrLf & "Do you want to re-encode it to MP4?", "Incompatible format", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                If result = DialogResult.Yes Then
                    UpdateStatusBox("♻️ Starting re-encoding to MP4...")
                    Dim reencodedFile As String = ReencodeVideo(targetFile)

                    If Not String.IsNullOrEmpty(reencodedFile) Then
                        UpdateStatusBox("✅ Re-encoding complete. Applying thumbnail...")
                        Return ApplyThumbnailToFile(reencodedFile) ' Apply thumbnail to the re-encoded file
                    Else
                        UpdateStatusBox("❌ Re-encoding failed. The original file is preserved.")
                        Return False
                    End If
                Else
                    UpdateStatusBox("⏭️ Re-encoding canceled by the user. No changes made.")
                    Return False
                End If
            End If

            ' Apply the thumbnail normally
            Dim tempOutputFile As String = Path.Combine(Path.GetTempPath(), Path.GetFileName(targetFile) & "_tmp.mp4")
            Dim cmd As String = $"-i ""{targetFile}"" -i ""{tempThumbnailPath}"" -map 0 -map 1 -c copy -disposition:v:1 attached_pic ""{tempOutputFile}"""

            ProgressBar2.Value = 0
            ProgressLabel.Text = "📂 Adding thumbnail..."
            UpdateStatusBox("🚀 Starting FFmpeg process to add thumbnail.")

            Dim process As New Process() With {
            .StartInfo = New ProcessStartInfo(ffmpegPath, cmd) With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }
        }
            process.Start()

            ' Read FFmpeg logs for progress tracking
            Dim outputReader As StreamReader = process.StandardError
            While Not process.HasExited
                Dim line As String = outputReader.ReadLine()
                If line IsNot Nothing AndAlso line.Contains("frame=") Then
                    ProgressBar2.Value = Math.Min(ProgressBar2.Value + 5, 80)
                    ProgressLabel.Text = $"Adding thumbnail... {ProgressBar2.Value}%"
                    UpdateStatusBox(line)
                End If
            End While
            process.WaitForExit()

            ' Check if the thumbnail was added successfully
            If File.Exists(tempOutputFile) AndAlso IsFileReadable(tempOutputFile) Then
                PreserveFileAttributes(targetFile, tempOutputFile)
                File.Move(tempOutputFile, targetFile, True)

                ProgressBar2.Value = 100
                ProgressLabel.Text = "✅ Operation complete"
                UpdateStatusBox("🎉 Thumbnail applied successfully!")
                Return True
            Else
                UpdateStatusBox("❌ Error applying thumbnail.")
                Return False
            End If
        Catch ex As Exception
            UpdateStatusBox("❌ Error processing: " & ex.Message)
            Return False
        End Try
    End Function

    Private Function ReencodeVideo(targetFile As String) As String
        Try
            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then
                UpdateStatusBox("❌ FFmpeg not found.")
                Return ""
            End If

            ' Determine the total duration of the video
            Dim totalDuration As Integer = GetVideoDuration(targetFile)
            If totalDuration = 0 Then
                UpdateStatusBox("❌ Unable to retrieve video duration.")
                Return ""
            End If

            ' Generate the name of the re-encoded MP4 file
            Dim reencodedFile As String = Path.Combine(Path.GetDirectoryName(targetFile),
                                    Path.GetFileNameWithoutExtension(targetFile) & "_reencoded.mp4")

            Dim cmd As String = $"-i ""{targetFile}"" -c:v libx264 -preset slow -crf 18 -c:a aac -b:a 192k ""{reencodedFile}"""

            ProgressBar2.Value = 0
            ProgressLabel.Text = "🔄 Re-encoding in progress..."
            UpdateStatusBox("♻️ Starting re-encoding...")

            ' Initialize the FFmpeg process
            Dim process As New Process() With {
            .StartInfo = New ProcessStartInfo(ffmpegPath, cmd) With {
                .CreateNoWindow = True,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }
        }
            process.Start()

            ' Read FFmpeg logs for progress tracking
            Dim outputReader As StreamReader = process.StandardError
            While Not process.HasExited
                Dim line As String = outputReader.ReadLine()
                If line IsNot Nothing AndAlso line.Contains("time=") Then
                    ' Extract the elapsed time from the line
                    Dim match = System.Text.RegularExpressions.Regex.Match(line, "time=(\d+):(\d+):(\d+).(\d+)")
                    If match.Success Then
                        Dim hours As Integer = Integer.Parse(match.Groups(1).Value)
                        Dim minutes As Integer = Integer.Parse(match.Groups(2).Value)
                        Dim seconds As Integer = Integer.Parse(match.Groups(3).Value)
                        Dim elapsedTime As Integer = (hours * 3600) + (minutes * 60) + seconds

                        ' Calculate the progress percentage
                        Dim progress As Integer = CInt((elapsedTime / totalDuration) * 100)
                        ProgressBar2.Value = Math.Min(progress, 100)
                        ProgressLabel.Text = $"Re-encoding... {ProgressBar2.Value}%"
                    End If
                    UpdateStatusBox(line)
                End If
            End While
            process.WaitForExit()

            ' Verify the re-encoded file
            If File.Exists(reencodedFile) AndAlso IsFileReadable(reencodedFile) Then
                ProgressBar2.Value = 100
                ProgressLabel.Text = "✅ Re-encoding complete"
                UpdateStatusBox("🎥 Video re-encoded successfully!")
                Return reencodedFile
            Else
                UpdateStatusBox("❌ Error re-encoding.")
                Return ""
            End If

        Catch ex As Exception
            UpdateStatusBox("❌ Error re-encoding: " & ex.Message)
            Return ""
        End Try
    End Function

    ' Function to preserve file attributes
    Private Sub PreserveFileAttributes(originalFile As String, newFile As String)
        Dim originalInfo As FileInfo = New FileInfo(originalFile)
        Dim newInfo As FileInfo = New FileInfo(newFile)

        Try
            newInfo.CreationTime = originalInfo.CreationTime
            newInfo.LastAccessTime = originalInfo.LastAccessTime
            newInfo.LastWriteTime = originalInfo.LastWriteTime
            newInfo.Attributes = originalInfo.Attributes
            UpdateStatusBox("✅ Attributes preserved on the updated file.")
        Catch ex As Exception
            UpdateStatusBox("⚠️ Unable to copy attributes: " & ex.Message)
        End Try
    End Sub

    ' Function to add text to the log without overwriting existing content
    Private Sub UpdateStatusBox(message As String)
        StatusBox.AppendText(message & vbCrLf)
    End Sub

End Class
