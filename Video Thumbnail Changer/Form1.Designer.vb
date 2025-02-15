<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        VideoGrid = New FlowLayoutPanel()
        WindowsIconThumbnail = New PictureBox()
        TakeScreenShot = New Button()
        FinalPictureBox = New PictureBox()
        VideoPanel = New Panel()
        Video = New AxWMPLib.AxWindowsMediaPlayer()
        PictureBox1 = New PictureBox()
        PictureBox2 = New PictureBox()
        PictureBox3 = New PictureBox()
        PictureBox4 = New PictureBox()
        PictureBox5 = New PictureBox()
        PictureBox6 = New PictureBox()
        PictureBox7 = New PictureBox()
        PictureBox8 = New PictureBox()
        PictureBox9 = New PictureBox()
        PictureBox10 = New PictureBox()
        ApplyToVidFile = New Button()
        btnSelectFolder = New Button()
        FFmpegPath = New TextBox()
        BrowseFFmpeg = New Button()
        FolderPath = New TextBox()
        VideoFlowPanel = New Panel()
        Label6 = New Label()
        IcarosPathTextbox = New TextBox()
        DLIcarosButton = New Button()
        BrowseIcarosButton = New Button()
        CustomOverlay = New TextBox()
        ColorDialog1 = New ColorDialog()
        StatusBox = New TextBox()
        BGTextOverlay = New TextBox()
        FontTextOverlay = New TextBox()
        DLFFmpegButton = New Button()
        ActualFileName = New TextBox()
        ThumbnailEdition = New GroupBox()
        ResetOverlay = New Button()
        MarginBorder = New TextBox()
        Label5 = New Label()
        Label4 = New Label()
        Label3 = New Label()
        OverlaySize = New TextBox()
        TextPositionCombobox = New ComboBox()
        Label2 = New Label()
        Label1 = New Label()
        GroupThumb = New GroupBox()
        VideoFileActionsGroup = New GroupBox()
        ReencodeCheckbox = New CheckBox()
        ProgressLabel = New TextBox()
        ProgressBar2 = New ProgressBar()
        MergeScreen = New Button()
        MergeMethodCombobox = New ComboBox()
        Label7 = New Label()
        ImageList1 = New ImageList(components)
        ThumbnailFlowPanel = New FlowLayoutPanel()
        VideoFullTimeTextbox = New TextBox()
        Label8 = New Label()
        CType(WindowsIconThumbnail, ComponentModel.ISupportInitialize).BeginInit()
        CType(FinalPictureBox, ComponentModel.ISupportInitialize).BeginInit()
        VideoPanel.SuspendLayout()
        CType(Video, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox8, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox10, ComponentModel.ISupportInitialize).BeginInit()
        VideoFlowPanel.SuspendLayout()
        ThumbnailEdition.SuspendLayout()
        GroupThumb.SuspendLayout()
        VideoFileActionsGroup.SuspendLayout()
        SuspendLayout()
        ' 
        ' VideoGrid
        ' 
        VideoGrid.AutoSizeMode = AutoSizeMode.GrowAndShrink
        VideoGrid.BackColor = Color.Lavender
        VideoGrid.Cursor = Cursors.Hand
        VideoGrid.FlowDirection = FlowDirection.TopDown
        VideoGrid.Location = New Point(3, 32)
        VideoGrid.Name = "VideoGrid"
        VideoGrid.Size = New Size(305, 336)
        VideoGrid.TabIndex = 2
        VideoGrid.WrapContents = False
        ' 
        ' WindowsIconThumbnail
        ' 
        WindowsIconThumbnail.Location = New Point(767, 209)
        WindowsIconThumbnail.Name = "WindowsIconThumbnail"
        WindowsIconThumbnail.Size = New Size(229, 124)
        WindowsIconThumbnail.SizeMode = PictureBoxSizeMode.Zoom
        WindowsIconThumbnail.TabIndex = 3
        WindowsIconThumbnail.TabStop = False
        ' 
        ' TakeScreenShot
        ' 
        TakeScreenShot.BackColor = Color.SkyBlue
        TakeScreenShot.Location = New Point(280, 301)
        TakeScreenShot.Name = "TakeScreenShot"
        TakeScreenShot.Size = New Size(152, 29)
        TakeScreenShot.TabIndex = 7
        TakeScreenShot.Text = "3) Take ScreenShot"
        TakeScreenShot.UseVisualStyleBackColor = False
        ' 
        ' FinalPictureBox
        ' 
        FinalPictureBox.Location = New Point(767, 535)
        FinalPictureBox.Name = "FinalPictureBox"
        FinalPictureBox.Size = New Size(229, 124)
        FinalPictureBox.SizeMode = PictureBoxSizeMode.Zoom
        FinalPictureBox.TabIndex = 8
        FinalPictureBox.TabStop = False
        ' 
        ' VideoPanel
        ' 
        VideoPanel.Controls.Add(TakeScreenShot)
        VideoPanel.Controls.Add(Video)
        VideoPanel.Location = New Point(9, 156)
        VideoPanel.Name = "VideoPanel"
        VideoPanel.Size = New Size(432, 333)
        VideoPanel.TabIndex = 9
        ' 
        ' Video
        ' 
        Video.Enabled = True
        Video.Location = New Point(6, 5)
        Video.Name = "Video"
        Video.OcxState = CType(resources.GetObject("Video.OcxState"), AxHost.State)
        Video.Size = New Size(423, 323)
        Video.TabIndex = 8
        ' 
        ' PictureBox1
        ' 
        PictureBox1.Location = New Point(6, 26)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(139, 75)
        PictureBox1.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox1.TabIndex = 10
        PictureBox1.TabStop = False
        ' 
        ' PictureBox2
        ' 
        PictureBox2.Location = New Point(156, 26)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(139, 75)
        PictureBox2.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox2.TabIndex = 11
        PictureBox2.TabStop = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.Location = New Point(306, 26)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(139, 75)
        PictureBox3.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox3.TabIndex = 12
        PictureBox3.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.Location = New Point(455, 26)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(139, 75)
        PictureBox4.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox4.TabIndex = 13
        PictureBox4.TabStop = False
        ' 
        ' PictureBox5
        ' 
        PictureBox5.Location = New Point(607, 26)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New Size(139, 75)
        PictureBox5.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox5.TabIndex = 14
        PictureBox5.TabStop = False
        ' 
        ' PictureBox6
        ' 
        PictureBox6.Location = New Point(5, 127)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(139, 75)
        PictureBox6.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox6.TabIndex = 19
        PictureBox6.TabStop = False
        ' 
        ' PictureBox7
        ' 
        PictureBox7.Location = New Point(155, 127)
        PictureBox7.Name = "PictureBox7"
        PictureBox7.Size = New Size(139, 75)
        PictureBox7.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox7.TabIndex = 18
        PictureBox7.TabStop = False
        ' 
        ' PictureBox8
        ' 
        PictureBox8.Location = New Point(305, 127)
        PictureBox8.Name = "PictureBox8"
        PictureBox8.Size = New Size(139, 75)
        PictureBox8.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox8.TabIndex = 17
        PictureBox8.TabStop = False
        ' 
        ' PictureBox9
        ' 
        PictureBox9.Location = New Point(454, 127)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(139, 75)
        PictureBox9.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox9.TabIndex = 16
        PictureBox9.TabStop = False
        ' 
        ' PictureBox10
        ' 
        PictureBox10.Location = New Point(606, 127)
        PictureBox10.Name = "PictureBox10"
        PictureBox10.Size = New Size(139, 75)
        PictureBox10.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox10.TabIndex = 15
        PictureBox10.TabStop = False
        ' 
        ' ApplyToVidFile
        ' 
        ApplyToVidFile.BackColor = Color.Salmon
        ApplyToVidFile.Location = New Point(6, 25)
        ApplyToVidFile.Name = "ApplyToVidFile"
        ApplyToVidFile.Size = New Size(44, 29)
        ApplyToVidFile.TabIndex = 20
        ApplyToVidFile.Text = "GO!"
        ApplyToVidFile.UseVisualStyleBackColor = False
        ' 
        ' btnSelectFolder
        ' 
        btnSelectFolder.BackColor = Color.SkyBlue
        btnSelectFolder.Location = New Point(-1, 121)
        btnSelectFolder.Name = "btnSelectFolder"
        btnSelectFolder.Size = New Size(245, 29)
        btnSelectFolder.TabIndex = 22
        btnSelectFolder.Text = "1) Browse VideoPath"
        btnSelectFolder.UseVisualStyleBackColor = False
        ' 
        ' FFmpegPath
        ' 
        FFmpegPath.Location = New Point(250, 59)
        FFmpegPath.Name = "FFmpegPath"
        FFmpegPath.Size = New Size(746, 27)
        FFmpegPath.TabIndex = 1
        ' 
        ' BrowseFFmpeg
        ' 
        BrowseFFmpeg.Location = New Point(-1, 59)
        BrowseFFmpeg.Name = "BrowseFFmpeg"
        BrowseFFmpeg.Size = New Size(118, 29)
        BrowseFFmpeg.TabIndex = 24
        BrowseFFmpeg.Text = "BrowseFFmpeg"
        BrowseFFmpeg.UseVisualStyleBackColor = True
        ' 
        ' FolderPath
        ' 
        FolderPath.Location = New Point(250, 123)
        FolderPath.Name = "FolderPath"
        FolderPath.Size = New Size(746, 27)
        FolderPath.TabIndex = 1
        ' 
        ' VideoFlowPanel
        ' 
        VideoFlowPanel.BackColor = Color.Gainsboro
        VideoFlowPanel.Controls.Add(Label6)
        VideoFlowPanel.Controls.Add(VideoGrid)
        VideoFlowPanel.Location = New Point(447, 161)
        VideoFlowPanel.Name = "VideoFlowPanel"
        VideoFlowPanel.Size = New Size(314, 375)
        VideoFlowPanel.TabIndex = 26
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.BackColor = Color.SkyBlue
        Label6.Location = New Point(7, 9)
        Label6.Name = "Label6"
        Label6.Size = New Size(228, 20)
        Label6.TabIndex = 44
        Label6.Text = "2) Clic the video you want to Edit"
        ' 
        ' IcarosPathTextbox
        ' 
        IcarosPathTextbox.Location = New Point(250, 88)
        IcarosPathTextbox.Name = "IcarosPathTextbox"
        IcarosPathTextbox.Size = New Size(746, 27)
        IcarosPathTextbox.TabIndex = 29
        ' 
        ' DLIcarosButton
        ' 
        DLIcarosButton.BackColor = Color.DarkSeaGreen
        DLIcarosButton.Location = New Point(123, 88)
        DLIcarosButton.Name = "DLIcarosButton"
        DLIcarosButton.Size = New Size(121, 29)
        DLIcarosButton.TabIndex = 28
        DLIcarosButton.Text = "b) DL Icaros"
        DLIcarosButton.TextAlign = ContentAlignment.MiddleLeft
        DLIcarosButton.UseVisualStyleBackColor = False
        ' 
        ' BrowseIcarosButton
        ' 
        BrowseIcarosButton.Location = New Point(-1, 88)
        BrowseIcarosButton.Name = "BrowseIcarosButton"
        BrowseIcarosButton.Size = New Size(118, 29)
        BrowseIcarosButton.TabIndex = 30
        BrowseIcarosButton.Text = "Browse Icaros"
        BrowseIcarosButton.UseVisualStyleBackColor = True
        ' 
        ' CustomOverlay
        ' 
        CustomOverlay.Location = New Point(6, 127)
        CustomOverlay.Name = "CustomOverlay"
        CustomOverlay.Size = New Size(119, 27)
        CustomOverlay.TabIndex = 31
        CustomOverlay.Text = "JPG"
        CustomOverlay.TextAlign = HorizontalAlignment.Center
        ' 
        ' StatusBox
        ' 
        StatusBox.BackColor = SystemColors.Info
        StatusBox.Location = New Point(1017, 1)
        StatusBox.Multiline = True
        StatusBox.Name = "StatusBox"
        StatusBox.ReadOnly = True
        StatusBox.ScrollBars = ScrollBars.Vertical
        StatusBox.Size = New Size(438, 785)
        StatusBox.TabIndex = 1
        ' 
        ' BGTextOverlay
        ' 
        BGTextOverlay.Location = New Point(80, 23)
        BGTextOverlay.Name = "BGTextOverlay"
        BGTextOverlay.Size = New Size(41, 27)
        BGTextOverlay.TabIndex = 32
        ' 
        ' FontTextOverlay
        ' 
        FontTextOverlay.Location = New Point(80, 59)
        FontTextOverlay.Name = "FontTextOverlay"
        FontTextOverlay.Size = New Size(41, 27)
        FontTextOverlay.TabIndex = 33
        ' 
        ' DLFFmpegButton
        ' 
        DLFFmpegButton.BackColor = Color.DarkSeaGreen
        DLFFmpegButton.Location = New Point(123, 59)
        DLFFmpegButton.Name = "DLFFmpegButton"
        DLFFmpegButton.Size = New Size(121, 29)
        DLFFmpegButton.TabIndex = 34
        DLFFmpegButton.Text = "a) DL FFmpeg"
        DLFFmpegButton.TextAlign = ContentAlignment.MiddleLeft
        DLFFmpegButton.UseVisualStyleBackColor = False
        ' 
        ' ActualFileName
        ' 
        ActualFileName.BackColor = SystemColors.Highlight
        ActualFileName.ForeColor = SystemColors.Window
        ActualFileName.Location = New Point(767, 157)
        ActualFileName.Multiline = True
        ActualFileName.Name = "ActualFileName"
        ActualFileName.Size = New Size(229, 46)
        ActualFileName.TabIndex = 35
        ActualFileName.TextAlign = HorizontalAlignment.Center
        ' 
        ' ThumbnailEdition
        ' 
        ThumbnailEdition.Controls.Add(ResetOverlay)
        ThumbnailEdition.Controls.Add(MarginBorder)
        ThumbnailEdition.Controls.Add(Label5)
        ThumbnailEdition.Controls.Add(Label4)
        ThumbnailEdition.Controls.Add(Label3)
        ThumbnailEdition.Controls.Add(OverlaySize)
        ThumbnailEdition.Controls.Add(TextPositionCombobox)
        ThumbnailEdition.Controls.Add(Label2)
        ThumbnailEdition.Controls.Add(Label1)
        ThumbnailEdition.Controls.Add(CustomOverlay)
        ThumbnailEdition.Controls.Add(BGTextOverlay)
        ThumbnailEdition.Controls.Add(FontTextOverlay)
        ThumbnailEdition.Location = New Point(767, 369)
        ThumbnailEdition.Name = "ThumbnailEdition"
        ThumbnailEdition.Size = New Size(229, 160)
        ThumbnailEdition.TabIndex = 36
        ThumbnailEdition.TabStop = False
        ThumbnailEdition.Text = "Overlay Edition"
        ' 
        ' ResetOverlay
        ' 
        ResetOverlay.BackColor = Color.Yellow
        ResetOverlay.Location = New Point(131, 125)
        ResetOverlay.Name = "ResetOverlay"
        ResetOverlay.Size = New Size(98, 29)
        ResetOverlay.TabIndex = 47
        ResetOverlay.Text = "Reset"
        ResetOverlay.UseVisualStyleBackColor = False
        ' 
        ' MarginBorder
        ' 
        MarginBorder.Location = New Point(182, 56)
        MarginBorder.Name = "MarginBorder"
        MarginBorder.Size = New Size(41, 27)
        MarginBorder.TabIndex = 43
        MarginBorder.Text = "15"
        MarginBorder.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(120, 63)
        Label5.Name = "Label5"
        Label5.Size = New Size(56, 20)
        Label5.TabIndex = 42
        Label5.Text = "Margin"
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(6, 95)
        Label4.Name = "Label4"
        Label4.Size = New Size(61, 20)
        Label4.TabIndex = 41
        Label4.Text = "Position"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(140, 23)
        Label3.Name = "Label3"
        Label3.Size = New Size(36, 20)
        Label3.TabIndex = 40
        Label3.Text = "Size"
        ' 
        ' OverlaySize
        ' 
        OverlaySize.Location = New Point(182, 23)
        OverlaySize.Name = "OverlaySize"
        OverlaySize.Size = New Size(41, 27)
        OverlaySize.TabIndex = 39
        OverlaySize.Text = "40"
        OverlaySize.TextAlign = HorizontalAlignment.Center
        ' 
        ' TextPositionCombobox
        ' 
        TextPositionCombobox.FormattingEnabled = True
        TextPositionCombobox.Location = New Point(73, 92)
        TextPositionCombobox.Name = "TextPositionCombobox"
        TextPositionCombobox.Size = New Size(150, 28)
        TextPositionCombobox.TabIndex = 38
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(6, 63)
        Label2.Name = "Label2"
        Label2.Size = New Size(68, 20)
        Label2.TabIndex = 35
        Label2.Text = "Txt Color"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(6, 23)
        Label1.Name = "Label1"
        Label1.Size = New Size(68, 20)
        Label1.TabIndex = 34
        Label1.Text = "BG Color"
        ' 
        ' GroupThumb
        ' 
        GroupThumb.BackColor = Color.FromArgb(CByte(255), CByte(242), CByte(255))
        GroupThumb.Controls.Add(PictureBox1)
        GroupThumb.Controls.Add(PictureBox4)
        GroupThumb.Controls.Add(PictureBox3)
        GroupThumb.Controls.Add(PictureBox2)
        GroupThumb.Controls.Add(PictureBox6)
        GroupThumb.Controls.Add(PictureBox9)
        GroupThumb.Controls.Add(PictureBox8)
        GroupThumb.Controls.Add(PictureBox10)
        GroupThumb.Controls.Add(PictureBox5)
        GroupThumb.Controls.Add(PictureBox7)
        GroupThumb.Location = New Point(9, 584)
        GroupThumb.Name = "GroupThumb"
        GroupThumb.Size = New Size(752, 214)
        GroupThumb.TabIndex = 37
        GroupThumb.TabStop = False
        GroupThumb.Text = "Thumbnails from"
        ' 
        ' VideoFileActionsGroup
        ' 
        VideoFileActionsGroup.BackColor = Color.LightSkyBlue
        VideoFileActionsGroup.Controls.Add(ReencodeCheckbox)
        VideoFileActionsGroup.Controls.Add(ProgressLabel)
        VideoFileActionsGroup.Controls.Add(ProgressBar2)
        VideoFileActionsGroup.Controls.Add(ApplyToVidFile)
        VideoFileActionsGroup.Location = New Point(767, 665)
        VideoFileActionsGroup.Name = "VideoFileActionsGroup"
        VideoFileActionsGroup.Size = New Size(229, 133)
        VideoFileActionsGroup.TabIndex = 38
        VideoFileActionsGroup.TabStop = False
        VideoFileActionsGroup.Text = "6) VideoFile Actions"
        ' 
        ' ReencodeCheckbox
        ' 
        ReencodeCheckbox.AutoSize = True
        ReencodeCheckbox.Checked = True
        ReencodeCheckbox.CheckState = CheckState.Checked
        ReencodeCheckbox.Font = New Font("Segoe UI", 7.6F)
        ReencodeCheckbox.Location = New Point(50, 31)
        ReencodeCheckbox.Name = "ReencodeCheckbox"
        ReencodeCheckbox.Size = New Size(173, 21)
        ReencodeCheckbox.TabIndex = 48
        ReencodeCheckbox.Text = "FFMPEG Reencode if Fail"
        ReencodeCheckbox.UseVisualStyleBackColor = True
        ' 
        ' ProgressLabel
        ' 
        ProgressLabel.BackColor = Color.LightSkyBlue
        ProgressLabel.Location = New Point(6, 63)
        ProgressLabel.Name = "ProgressLabel"
        ProgressLabel.Size = New Size(222, 27)
        ProgressLabel.TabIndex = 44
        ' 
        ' ProgressBar2
        ' 
        ProgressBar2.Location = New Point(5, 96)
        ProgressBar2.Name = "ProgressBar2"
        ProgressBar2.Size = New Size(223, 29)
        ProgressBar2.TabIndex = 43
        ' 
        ' MergeScreen
        ' 
        MergeScreen.BackColor = Color.BurlyWood
        MergeScreen.Location = New Point(616, 548)
        MergeScreen.Name = "MergeScreen"
        MergeScreen.Size = New Size(138, 29)
        MergeScreen.TabIndex = 41
        MergeScreen.Text = "Merge Screens"
        MergeScreen.UseVisualStyleBackColor = False
        MergeScreen.Visible = False
        ' 
        ' MergeMethodCombobox
        ' 
        MergeMethodCombobox.FormattingEnabled = True
        MergeMethodCombobox.Location = New Point(450, 549)
        MergeMethodCombobox.Name = "MergeMethodCombobox"
        MergeMethodCombobox.Size = New Size(151, 28)
        MergeMethodCombobox.TabIndex = 42
        MergeMethodCombobox.Visible = False
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.BackColor = Color.SkyBlue
        Label7.Location = New Point(785, 346)
        Label7.Name = "Label7"
        Label7.Size = New Size(194, 20)
        Label7.TabIndex = 45
        Label7.Text = "5) Add an Overlay if wanted"
        ' 
        ' ImageList1
        ' 
        ImageList1.ColorDepth = ColorDepth.Depth32Bit
        ImageList1.ImageSize = New Size(16, 16)
        ImageList1.TransparentColor = Color.Transparent
        ' 
        ' ThumbnailFlowPanel
        ' 
        ThumbnailFlowPanel.AutoScroll = True
        ThumbnailFlowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink
        ThumbnailFlowPanel.BackColor = Color.AntiqueWhite
        ThumbnailFlowPanel.Location = New Point(9, 496)
        ThumbnailFlowPanel.Name = "ThumbnailFlowPanel"
        ThumbnailFlowPanel.Size = New Size(432, 82)
        ThumbnailFlowPanel.TabIndex = 46
        ThumbnailFlowPanel.WrapContents = False
        ' 
        ' VideoFullTimeTextbox
        ' 
        VideoFullTimeTextbox.BackColor = SystemColors.Highlight
        VideoFullTimeTextbox.ForeColor = SystemColors.Window
        VideoFullTimeTextbox.Location = New Point(923, 209)
        VideoFullTimeTextbox.Multiline = True
        VideoFullTimeTextbox.Name = "VideoFullTimeTextbox"
        VideoFullTimeTextbox.ReadOnly = True
        VideoFullTimeTextbox.Size = New Size(73, 21)
        VideoFullTimeTextbox.TabIndex = 47
        VideoFullTimeTextbox.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Font = New Font("Segoe UI", 25.8000011F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label8.Location = New Point(117, -4)
        Label8.Name = "Label8"
        Label8.Size = New Size(700, 60)
        Label8.TabIndex = 48
        Label8.Text = "VIDEO THUMBNAIL GENERATOR"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8.0F, 20.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.AliceBlue
        ClientSize = New Size(1447, 806)
        Controls.Add(Label8)
        Controls.Add(VideoFullTimeTextbox)
        Controls.Add(ThumbnailFlowPanel)
        Controls.Add(Label7)
        Controls.Add(MergeMethodCombobox)
        Controls.Add(MergeScreen)
        Controls.Add(VideoFileActionsGroup)
        Controls.Add(GroupThumb)
        Controls.Add(ThumbnailEdition)
        Controls.Add(ActualFileName)
        Controls.Add(DLFFmpegButton)
        Controls.Add(StatusBox)
        Controls.Add(BrowseIcarosButton)
        Controls.Add(IcarosPathTextbox)
        Controls.Add(DLIcarosButton)
        Controls.Add(VideoFlowPanel)
        Controls.Add(FolderPath)
        Controls.Add(BrowseFFmpeg)
        Controls.Add(FFmpegPath)
        Controls.Add(btnSelectFolder)
        Controls.Add(VideoPanel)
        Controls.Add(FinalPictureBox)
        Controls.Add(WindowsIconThumbnail)
        Name = "Form1"
        Text = "Video Thumbnail Changer v1.2 - by soaresden"
        CType(WindowsIconThumbnail, ComponentModel.ISupportInitialize).EndInit()
        CType(FinalPictureBox, ComponentModel.ISupportInitialize).EndInit()
        VideoPanel.ResumeLayout(False)
        CType(Video, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox8, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox10, ComponentModel.ISupportInitialize).EndInit()
        VideoFlowPanel.ResumeLayout(False)
        VideoFlowPanel.PerformLayout()
        ThumbnailEdition.ResumeLayout(False)
        ThumbnailEdition.PerformLayout()
        GroupThumb.ResumeLayout(False)
        VideoFileActionsGroup.ResumeLayout(False)
        VideoFileActionsGroup.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents VideoGrid As FlowLayoutPanel
    Friend WithEvents WindowsIconThumbnail As PictureBox
    Friend WithEvents TakeScreenShot As Button
    Friend WithEvents FinalPictureBox As PictureBox
    Friend WithEvents VideoPanel As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents PictureBox7 As PictureBox
    Friend WithEvents PictureBox8 As PictureBox
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents PictureBox10 As PictureBox
    Friend WithEvents ApplyToVidFile As Button
    Friend WithEvents btnSelectFolder As Button
    Friend WithEvents FFmpegPath As TextBox
    Friend WithEvents BrowseFFmpeg As Button
    Friend WithEvents FolderPath As TextBox
    Friend WithEvents VideoFlowPanel As Panel
    Friend WithEvents IcarosPathTextbox As TextBox
    Friend WithEvents DLIcarosButton As Button
    Friend WithEvents BrowseIcarosButton As Button
    Friend WithEvents CustomOverlay As TextBox
    Friend WithEvents ColorDialog1 As ColorDialog
    Friend WithEvents StatusBox As TextBox
    Friend WithEvents BGTextOverlay As TextBox
    Friend WithEvents FontTextOverlay As TextBox
    Friend WithEvents DLFFmpegButton As Button
    Friend WithEvents ActualFileName As TextBox
    Friend WithEvents ThumbnailEdition As GroupBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents GroupThumb As GroupBox
    Friend WithEvents TextPositionCombobox As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents OverlaySize As TextBox
    Friend WithEvents MarginBorder As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents VideoFileActionsGroup As GroupBox
    Friend WithEvents MergeScreen As Button
    Friend WithEvents MergeMethodCombobox As ComboBox
    Friend WithEvents ProgressBar2 As ProgressBar
    Friend WithEvents ProgressLabel As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents ThumbnailFlowPanel As FlowLayoutPanel
    Friend WithEvents ResetOverlay As Button
    Friend WithEvents ReencodeCheckbox As CheckBox
    Friend WithEvents VideoFullTimeTextbox As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents Video As AxWMPLib.AxWindowsMediaPlayer

End Class
