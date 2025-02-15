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

        FinalPictureBox.Image = Nothing

        If My.Settings.ffmpegpath IsNot Nothing Then
            FFmpegPath.Text = My.Settings.ffmpegpath
        End If

        UpdateCustomOverlay()
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

        ' Add text position options
        TextPositionCombobox.Items.AddRange({"Up Left", "Up Middle", "Up Right", "Bottom Left", "Bottom Middle", "Bottom Right", "Screen Center"})
        TextPositionCombobox.SelectedIndex = 4 ' Default selection Bottom Middle

        Dim CheckBoxIconInThumbnail As New CheckBox With {
            .Text = "Icon in Thumbnail",
            .Name = "IconInThumbnail",
            .Checked = False,
            .AutoSize = True,
            .Location = New Point(10, 10)
        }
        AddHandler CheckBoxIconInThumbnail.CheckedChanged, AddressOf IconInThumbnail_CheckedChanged
        Me.Controls.Add(CheckBoxIconInThumbnail)

        UpdateStatusBox("Welcome to Video Thumbnail Changer ! ")
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        FolderPath.Focus()
    End Sub

    ' Update StatusBox with messages
    Private Sub UpdateStatusBox(message As String)
        StatusBox.Text = message
    End Sub

    ' Get FFmpeg Path
    Private Function GetFFMPEGPath() As String
        ' Check if a path is already set in the interface and if it exists
        If Not String.IsNullOrEmpty(FFmpegPath.Text) AndAlso File.Exists(FFmpegPath.Text) Then
            Return FFmpegPath.Text
        End If

        ' Directory where FFmpeg is supposed to be installed
        Dim ffmpegDirectory As String = Path.Combine(Application.StartupPath, "FFmpeg")

        ' Check if the FFmpeg directory exists
        If Directory.Exists(ffmpegDirectory) Then
            ' Recursively search for the ffmpeg.exe file in all subdirectories
            Dim ffmpegFiles As String() = Directory.GetFiles(ffmpegDirectory, "ffmpeg.exe", SearchOption.AllDirectories)

            ' If a file is found, return its full path
            If ffmpegFiles.Length > 0 Then
                Return ffmpegFiles(0)
            End If
        End If

        ' Return an empty string if FFmpeg is not found
        Return ""
    End Function

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
            Dim videoPanel As New Panel With {
                .Size = New Size(130, 140),
                .Padding = New Padding(5)
            }

            Dim pb As New PictureBox With {
                .Width = 120,
                .Height = 80,
                .SizeMode = PictureBoxSizeMode.StretchImage,
                .Tag = file
            }

            Dim lbl As New Label With {
                .Text = Path.GetFileNameWithoutExtension(file),
                .AutoSize = False,
                .Width = 120,
                .Height = 40,
                .TextAlign = ContentAlignment.MiddleCenter
            }

            pb.Image = GetWindowsVideoThumbnail(file)

            Dim tooltip As New ToolTip()
            Dim fileInfo As New FileInfo(file)
            tooltip.SetToolTip(pb, $"Name: {fileInfo.Name}{vbCrLf}Size: {fileInfo.Length \ 1024} KB{vbCrLf}Date: {fileInfo.CreationTime}")

            AddHandler pb.Click, AddressOf VideoClicked
            videoPanel.Controls.Add(pb)
            videoPanel.Controls.Add(lbl)
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

    ' Video Clicked Event
    Private Sub VideoClicked(sender As Object, e As EventArgs)
        Dim pb As PictureBox = TryCast(sender, PictureBox)
        If pb Is Nothing OrElse pb.Tag Is Nothing OrElse String.IsNullOrEmpty(pb.Tag.ToString()) Then
            UpdateStatusBox("Error: No video associated with this thumbnail.")
            Return
        End If

        selectedVideo = pb.Tag.ToString()

        ' Check if the video file exists
        If Not File.Exists(selectedVideo) Then
            UpdateStatusBox("The selected video does not exist.")
            Return
        End If

        ' Load video details
        ActualFileName.Text = Path.GetFileName(selectedVideo)
        GroupThumb.Text = "Thumbnails from " & ActualFileName.Text

        ' Check format compatibility for thumbnails
        Dim supportsThumbnails As Boolean = CheckThumbnailSupport(selectedVideo)

        ' Load the Windows-generated thumbnail (this always works)
        Dim windowsThumbnail As Image = GetWindowsVideoThumbnail(selectedVideo)
        If windowsThumbnail IsNot Nothing Then
            WindowsIconThumbnail.Image = windowsThumbnail
        End If

        ' If the format supports thumbnails, load them; otherwise, hide the UI elements
        If supportsThumbnails Then
            LoadThumbnails(selectedVideo)
            ThumbnailEdition.Visible = True
            JPGOverlay.Visible = True
            VideoFileActionsGroup.Visible = True
        Else
            ' Hide the thumbnail editing UI if not supported
            ThumbnailEdition.Visible = False
            JPGOverlay.Visible = False
            VideoFileActionsGroup.Visible = False
        End If

        ' Load the video into the player
        Try
            Video.URL = selectedVideo
            Video.Ctlcontrols.stop()
            Video.uiMode = "full"
            Video.settings.volume = 50
            Video.fullScreen = False
            AddHandler Video.PlayStateChange, AddressOf Video_PlayStateChanged

            Video.Ctlcontrols.play()
        Catch ex As Exception
            UpdateStatusBox("Error loading video: " & ex.Message)
        End Try
    End Sub


    Private Function CheckThumbnailSupport(filePath As String) As Boolean
        Dim supportedFormats As String() = {".mp4", ".mkv", ".avi"}
        Return supportedFormats.Contains(Path.GetExtension(filePath).ToLower())
    End Function

    ' Load Thumbnails for Video
    Private Sub LoadThumbnails(videoPath As String)
        ' Delete only the thumbnails that need to be replaced
        DeleteTemporaryScreenshots()

        Dim totalThumbnails As Integer = 10
        Dim videoDuration As Integer = GetVideoDuration(videoPath)
        If videoDuration = 0 Then Exit Sub

        Dim interval As Integer = videoDuration \ totalThumbnails
        Dim pictureBoxes As PictureBox() = {PictureBox1, PictureBox2, PictureBox3, PictureBox4, PictureBox5,
                                        PictureBox6, PictureBox7, PictureBox8, PictureBox9, PictureBox10}

        Dim tooltip As New ToolTip()

        ' Generate the main image
        Dim actualThumbPath As String = Path.Combine(Path.GetTempPath(), "actualthumb.jpg")
        If Not File.Exists(actualThumbPath) Then
            Dim mainThumb As Image = GetWindowsVideoThumbnail(videoPath)
            If mainThumb IsNot Nothing Then
                WindowsIconThumbnail.Image = New Bitmap(mainThumb) ' Copy to avoid locking

                ' 🟢 Add the Tag with an estimated timecode
                Dim estimatedTime As Integer = GetEstimatedWindowsThumbnailTimeCode(videoDuration)
                WindowsIconThumbnail.Tag = estimatedTime

                mainThumb.Dispose()
            End If
        End If

        ' Generate thumbnails for each PictureBox
        For i As Integer = 0 To totalThumbnails - 1
            Dim timestamp As Integer = i * interval
            pictureBoxes(i).Tag = timestamp  ' Store the timecode in the PictureBox
            AddHandler pictureBoxes(i).Click, AddressOf ThumbnailClicked ' Add click event handler

            Dim thumbnailPath As String = Path.Combine(Path.GetTempPath(), $"thumb{i + 1}.jpg")
            Dim thumbnail As Image = GetFFmpegVideoThumbnail(videoPath, timestamp, i + 1)
            If thumbnail IsNot Nothing Then
                Using tempImg As Bitmap = New Bitmap(thumbnail)
                    pictureBoxes(i).Image = New Bitmap(tempImg) ' Assign memory to avoid locking
                End Using
                tooltip.SetToolTip(pictureBoxes(i), $"Capture at {(timestamp / videoDuration) * 100}%")
            End If
        Next
    End Sub

    Private Function GetEstimatedWindowsThumbnailTimeCode(videoDuration As Integer) As Integer
        ' Use a constant factor of 0.25
        Dim factor As Double = 0.25

        ' Calculate the timecode
        Dim estimatedTime As Integer = Math.Max(0, Math.Min(CInt(videoDuration * factor), videoDuration - 1))

        ' 🟢 Debug: Display in the StatusBox
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

    Private Sub ThumbnailClicked(sender As Object, e As EventArgs)
        Dim pb As PictureBox = TryCast(sender, PictureBox)
        If pb IsNot Nothing AndAlso pb.Tag IsNot Nothing AndAlso IsNumeric(pb.Tag) Then
            Dim position As Integer = CInt(pb.Tag)

            If Video.currentMedia IsNot Nothing Then
                Video.Ctlcontrols.currentPosition = position
                Video.Ctlcontrols.play()
            Else
                UpdateStatusBox("Error: Unable to play the video.")
            End If
        End If
    End Sub

    Private Sub ScreenshotButton_Click(sender As Object, e As EventArgs) Handles TakeScreenShot.Click
        If String.IsNullOrEmpty(selectedVideo) Then
            UpdateStatusBox("No video is currently playing.")
            Return
        End If

        ' Capture the current screenshot from the video
        Dim timestamp As Integer = CInt(Video.Ctlcontrols.currentPosition)
        Dim screenshot As Image = GetFFmpegVideoThumbnail(selectedVideo, timestamp, 99)

        If screenshot IsNot Nothing Then
            ' Save a backup to restore later
            Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
            screenshot.Save(backupPath, Imaging.ImageFormat.Jpeg)

            ' Assign the image to FinalPictureBox
            FinalPictureBox.Image = New Bitmap(screenshot)

            ' If JPGOverlay is checked, apply the overlay immediately
            If JPGOverlay.Checked Then
                UpdateOverlay()
            End If

            UpdateStatusBox("Screenshot taken and displayed.")
        Else
            UpdateStatusBox("Error capturing image.")
        End If
    End Sub

    ' OVERLAY OPERATION
    ' Thumbnail Operation
    Private Sub JPGOverlay_CheckedChanged(sender As Object, e As EventArgs) Handles JPGOverlay.CheckedChanged
        If JPGOverlay.Checked Then
            ' Appliquer l'overlay sur l'image actuelle
            ThumbnailEdition.Visible = True
            UpdateOverlay()
        Else
            ' Charger immédiatement le backup sans overlay
            Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")

            If File.Exists(backupPath) Then
                Try
                    Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        FinalPictureBox.Image = New Bitmap(tempStream) ' Charger l'image de sauvegarde
                    End Using
                    UpdateStatusBox("Backup restauré après désactivation de l'overlay.")
                    ThumbnailEdition.Visible = False
                Catch ex As Exception
                    UpdateStatusBox("Erreur lors du chargement du backup: " & ex.Message)
                End Try
            Else
                UpdateStatusBox("Aucun backup trouvé, impossible de restaurer.")
            End If
        End If
    End Sub


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

    ' Temp Picture Box Click Event
    Private Sub FinalPictureBox_Click(sender As Object, e As EventArgs) Handles FinalPictureBox.Click
        If FinalPictureBox.Image IsNot Nothing Then
            ' Display the modified image with the overlay in a new window
            ShowFullSizeImage()
        End If
    End Sub

    ' Background Color Click Event
    Private Sub BGTextOverlay_Click(sender As Object, e As EventArgs) Handles BGTextOverlay.Click
        Using colorDialog As New ColorDialog()
            If colorDialog.ShowDialog() = DialogResult.OK Then
                Dim overlayBackgroundColor = colorDialog.Color
                BGTextOverlay.BackColor = overlayBackgroundColor
                UpdateCustomOverlay()
            End If
        End Using
    End Sub

    ' Font Color Click Event
    Private Sub FontTextOverlay_Click(sender As Object, e As EventArgs) Handles FontTextOverlay.Click
        Using colorDialog As New ColorDialog()
            If colorDialog.ShowDialog() = DialogResult.OK Then
                Dim overlayTextColor = colorDialog.Color
                FontTextOverlay.BackColor = overlayTextColor
                UpdateCustomOverlay()
            End If
        End Using
    End Sub

    Private Sub UpdateCustomOverlay()
        If JPGOverlay.Checked = True Then
            ThumbnailEdition.Visible = True
        Else
            ThumbnailEdition.Visible = False
        End If
    End Sub

    Private Function ReloadFromBackupAndApplyOverlay() As Image
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")

        If Not File.Exists(backupPath) Then
            UpdateStatusBox("Error: No backup found.")
            Return Nothing
        End If

        Dim backupImg As Image
        Try
            Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                backupImg = New Bitmap(tempStream)
            End Using
        Catch ex As Exception
            UpdateStatusBox("Error loading the backup: " & ex.Message)
            Return Nothing
        End Try

        ' Apply the overlay and return the final image
        Return ApplyOverlayToImage(backupImg)
    End Function

    Private Sub OverlaySize_TextChanged(sender As Object, e As EventArgs) Handles OverlaySize.TextChanged
        ' Check that the size is a valid number
        Dim fontSize As Integer
        If Integer.TryParse(OverlaySize.Text, fontSize) AndAlso fontSize > 0 Then
            If JPGOverlay.Checked Then
                UpdateOverlay()
            End If
        End If
    End Sub

    Private Sub TextPositionCombobox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TextPositionCombobox.SelectedIndexChanged
        ' Update the current position
        Select Case TextPositionCombobox.SelectedIndex
            Case 0 : currentTextPosition = TextPosition.UL
            Case 1 : currentTextPosition = TextPosition.UM
            Case 2 : currentTextPosition = TextPosition.UR
            Case 3 : currentTextPosition = TextPosition.BL
            Case 4 : currentTextPosition = TextPosition.BM
            Case 5 : currentTextPosition = TextPosition.BR
            Case 6 : currentTextPosition = TextPosition.MM
        End Select

        ' Check if JPGOverlay is checked before applying the overlay
        If JPGOverlay.Checked Then
            UpdateOverlay()
        End If
    End Sub

    Private Sub CustomOverlay_TextChanged(sender As Object, e As EventArgs) Handles CustomOverlay.TextChanged
        UpdateOverlay()
    End Sub

    Private Sub MarginBorder_TextChanged(sender As Object, e As EventArgs) Handles MarginBorder.TextChanged
        UpdateOverlay()
    End Sub

    Private Sub UpdateOverlay()
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")

        If Not File.Exists(backupPath) Then
            UpdateStatusBox("No backup image found.")
            Return
        End If

        ' Load the backup image
        Dim backupImg As Image
        Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            backupImg = New Bitmap(tempStream)
        End Using

        ' Apply the overlay
        Dim overlayedImg As Image = ApplyOverlayToImage(backupImg)

        ' Assign the final image and refresh
        FinalPictureBox.Image = overlayedImg
        FinalPictureBox.Refresh()
    End Sub

    Private Function ApplyOverlayToImage(originalImg As Image) As Image
        If originalImg Is Nothing Then Return Nothing

        Dim bmp As New Bitmap(originalImg)
        Using g As Graphics = Graphics.FromImage(bmp)
            Dim pos As Point

            ' Get the margin size from the textbox
            Dim BordersSize As Integer
            If Not Integer.TryParse(MarginBorder.Text, BordersSize) OrElse BordersSize <= 0 Then
                BordersSize = 15 ' Default value if invalid
            End If
            BordersSize = Math.Max(5, Math.Min(BordersSize, 100)) ' Clamp between 5 and 100

            ' Calculate proportional margins based on the image
            Dim MarginOffsetX As Integer = bmp.Width \ BordersSize
            Dim MarginOffsetY As Integer = bmp.Height \ BordersSize

            ' Get the font size
            Dim fontSize As Integer
            If Not Integer.TryParse(OverlaySize.Text, fontSize) OrElse fontSize <= 0 Then
                fontSize = 40 ' Default value if invalid
            End If
            fontSize = Math.Max(5, Math.Min(fontSize, 100))

            ' Calculate the font size based on the image height
            Dim fontRatio As Double = 40 / 360 ' 40px for a 360px high video
            Dim calculatedFontSize As Integer = CInt(bmp.Height * fontRatio)

            ' Prevent very small or very large values
            calculatedFontSize = Math.Max(5, Math.Min(calculatedFontSize, 100))

            ' Create the font and measure the text
            Dim font As Font = New Font("System", calculatedFontSize, FontStyle.Bold)
            Dim textSize As SizeF = g.MeasureString(CustomOverlay.Text, font)

            ' Dimensions of the rectangle around the text
            Dim padding As Integer = 10
            Dim rectWidth As Integer = CInt(textSize.Width) + padding * 2
            Dim rectHeight As Integer = CInt(textSize.Height) + padding * 2

            ' Add half of the margin to center equally (for MM)
            Dim halfMarginX As Integer = MarginOffsetX \ 2
            Dim halfMarginY As Integer = MarginOffsetY \ 2

            ' Position the rectangle based on the margin
            Select Case currentTextPosition
                Case TextPosition.UL ' Top Left
                    pos = New Point(MarginOffsetX, MarginOffsetY + rectHeight)
                Case TextPosition.UM ' Top Middle
                    pos = New Point((bmp.Width - rectWidth) \ 2, MarginOffsetY + rectHeight)
                Case TextPosition.UR ' Top Right
                    pos = New Point(bmp.Width - rectWidth - MarginOffsetX, MarginOffsetY + rectHeight)
                Case TextPosition.BL ' Bottom Left
                    pos = New Point(MarginOffsetX, bmp.Height - MarginOffsetY)
                Case TextPosition.BM ' Bottom Middle
                    pos = New Point((bmp.Width - rectWidth) \ 2, bmp.Height - MarginOffsetY)
                Case TextPosition.BR ' Bottom Right
                    pos = New Point(bmp.Width - rectWidth - MarginOffsetX, bmp.Height - MarginOffsetY)
                Case TextPosition.MM ' Center screen with balanced margin
                    pos = New Point((bmp.Width \ 2) - (rectWidth / 2), (bmp.Height \ 2) + (rectHeight / 2))
            End Select

            ' Final adjustment: rect.Y should be positioned above the text
            Dim rectX As Integer = pos.X
            Dim rectY As Integer = pos.Y - rectHeight ' Subtract to place the text correctly

            ' Check to avoid going out of the image
            rectX = Math.Max(0, Math.Min(bmp.Width - rectWidth, rectX))
            rectY = Math.Max(0, Math.Min(bmp.Height - rectHeight, rectY))

            ' Draw the background rectangle
            Dim rect As New Rectangle(rectX, rectY, rectWidth, rectHeight)
            g.FillRectangle(New SolidBrush(CustomOverlay.BackColor), rect)

            ' Draw the text centered within the rectangle
            Dim textX As Single = rect.X + (rect.Width - textSize.Width) / 2
            Dim textY As Single = rect.Y + (rect.Height - textSize.Height) / 2
            g.DrawString(CustomOverlay.Text, font, New SolidBrush(CustomOverlay.ForeColor), textX, textY)
        End Using

        Return bmp
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

    Private Sub UpdateFinalImage()
        Dim backupPath As String = Path.Combine(Path.GetTempPath(), "screenshot_backup.jpg")
        If Not File.Exists(backupPath) Then Exit Sub

        ' Load the backup image
        Dim backupImg As Image
        Using tempStream As New FileStream(backupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            backupImg = New Bitmap(tempStream)
        End Using

        ' Apply the overlay if the checkbox is checked
        If IconInThumbnail Then
            FinalPictureBox.Image = ApplyOverlayToImage(backupImg)
        Else
            FinalPictureBox.Image = backupImg
        End If
    End Sub

    ' Resize an image
    Private Function ResizeImageToMaxSize(originalImage As Bitmap, maxSize As Integer) As Bitmap
        Dim newWidth As Integer = originalImage.Width
        Dim newHeight As Integer = originalImage.Height

        If newWidth > newHeight AndAlso newWidth > maxSize Then
            newHeight = CInt(newHeight * (maxSize / newWidth))
            newWidth = maxSize
        ElseIf newHeight > maxSize Then
            newWidth = CInt(newWidth * (maxSize / newHeight))
            newHeight = maxSize
        End If

        Dim resizedBitmap As New Bitmap(newWidth, newHeight)
        Using g As Graphics = Graphics.FromImage(resizedBitmap)
            g.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(originalImage, 0, 0, newWidth, newHeight)
        End Using
        Return resizedBitmap
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
            UpdateStatusBox("Thumbnail applied successfully!")
        Else
            UpdateStatusBox("Error applying thumbnail.")
        End If

        RefreshExplorerThumbnail(selectedVideo)
        MsgBox("Successfully added to " & Path.GetFileName(selectedVideo))
    End Sub

    ' Apply Thumbnail New File Button Click Event
    Private Sub ApplyThumbnailNewFileButton_Click(sender As Object, e As EventArgs) Handles ApplyToNewFile.Click
        If String.IsNullOrEmpty(selectedVideo) OrElse FinalPictureBox.Image Is Nothing Then
            UpdateStatusBox("Select a video and a thumbnail before applying.")
            Return
        End If

        Dim newFilePath As String = Path.Combine(Path.GetDirectoryName(selectedVideo), Path.GetFileNameWithoutExtension(selectedVideo) & "_newthumb" & Path.GetExtension(selectedVideo))

        If File.Exists(newFilePath) Then
            File.Delete(newFilePath)
        End If

        File.Copy(selectedVideo, newFilePath)

        If ApplyThumbnailToFile(newFilePath) Then
            UpdateStatusBox("Thumbnail applied successfully to the copy!")
        Else
            UpdateStatusBox("Error applying thumbnail to the copy.")
        End If

        RefreshExplorerThumbnail(newFilePath)
        MsgBox("Successful copy: " & Path.GetFileNameWithoutExtension(newFilePath))
    End Sub

    ' Apply Thumbnail to File
    Private Function ApplyThumbnailToFile(targetFile As String) As Boolean
        Try
            Dim tempThumbnailPath As String = Path.Combine(Path.GetTempPath(), "new_thumbnail.jpg")
            FinalPictureBox.Image.Save(tempThumbnailPath, Imaging.ImageFormat.Jpeg)

            Dim ffmpegPath As String = Me.FFmpegPath.Text
            If Not File.Exists(ffmpegPath) Then
                UpdateStatusBox("FFmpeg file not found!")
                Return False
            End If

            Dim cmd As String = $"-i ""{targetFile}"" -i ""{tempThumbnailPath}"" -map 0 -map 1 -c copy -disposition:v:1 attached_pic ""{targetFile}_tmp.mp4"""
            Dim process As New Process()
            process.StartInfo.FileName = ffmpegPath
            process.StartInfo.Arguments = cmd
            process.StartInfo.CreateNoWindow = True
            process.StartInfo.UseShellExecute = False
            process.Start()
            process.WaitForExit()

            If File.Exists(targetFile & "_tmp.mp4") Then
                File.Delete(targetFile)
                File.Move(targetFile & "_tmp.mp4", targetFile)

                ClearThumbnailCache()

                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            UpdateStatusBox("Error applying thumbnail: " & ex.Message)
            Return False
        End Try
    End Function

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
            Try
                FinalPictureBox.Image.Save(backupPath, Imaging.ImageFormat.Jpeg)
            Catch ex As Exception
                UpdateStatusBox("Error saving the backup: " & ex.Message)
            End Try
        End If
    End Sub
End Class
