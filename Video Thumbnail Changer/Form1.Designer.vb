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
        ApplyToNewFile = New Button()
        btnSelectFolder = New Button()
        FFmpegPath = New TextBox()
        BrowseFFmpeg = New Button()
        FolderPath = New TextBox()
        VideoFlowPanel = New Panel()
        JPGOverlay = New CheckBox()
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
        VideoGrid.BackColor = SystemColors.ButtonHighlight
        VideoGrid.Location = New Point(7, 5)
        VideoGrid.Name = "VideoGrid"
        VideoGrid.Size = New Size(272, 364)
        VideoGrid.TabIndex = 2
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
        TakeScreenShot.Location = New Point(223, 341)
        TakeScreenShot.Name = "TakeScreenShot"
        TakeScreenShot.Size = New Size(206, 29)
        TakeScreenShot.TabIndex = 7
        TakeScreenShot.Text = "TakeScreenShot"
        TakeScreenShot.UseVisualStyleBackColor = True
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
        VideoPanel.Size = New Size(461, 376)
        VideoPanel.TabIndex = 9
        ' 
        ' Video
        ' 
        Video.Enabled = True
        Video.Location = New Point(3, 5)
        Video.Name = "Video"
        Video.OcxState = CType(resources.GetObject("Video.OcxState"), AxHost.State)
        Video.Size = New Size(455, 368)
        Video.TabIndex = 0
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
        PictureBox6.Location = New Point(6, 112)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(139, 75)
        PictureBox6.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox6.TabIndex = 19
        PictureBox6.TabStop = False
        ' 
        ' PictureBox7
        ' 
        PictureBox7.Location = New Point(156, 112)
        PictureBox7.Name = "PictureBox7"
        PictureBox7.Size = New Size(139, 75)
        PictureBox7.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox7.TabIndex = 18
        PictureBox7.TabStop = False
        ' 
        ' PictureBox8
        ' 
        PictureBox8.Location = New Point(306, 112)
        PictureBox8.Name = "PictureBox8"
        PictureBox8.Size = New Size(139, 75)
        PictureBox8.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox8.TabIndex = 17
        PictureBox8.TabStop = False
        ' 
        ' PictureBox9
        ' 
        PictureBox9.Location = New Point(455, 112)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(139, 75)
        PictureBox9.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox9.TabIndex = 16
        PictureBox9.TabStop = False
        ' 
        ' PictureBox10
        ' 
        PictureBox10.Location = New Point(607, 112)
        PictureBox10.Name = "PictureBox10"
        PictureBox10.Size = New Size(139, 75)
        PictureBox10.SizeMode = PictureBoxSizeMode.Zoom
        PictureBox10.TabIndex = 15
        PictureBox10.TabStop = False
        ' 
        ' ApplyToVidFile
        ' 
        ApplyToVidFile.Location = New Point(6, 26)
        ApplyToVidFile.Name = "ApplyToVidFile"
        ApplyToVidFile.Size = New Size(217, 29)
        ApplyToVidFile.TabIndex = 20
        ApplyToVidFile.Text = "ApplyToVidFile"
        ApplyToVidFile.UseVisualStyleBackColor = True
        ' 
        ' ApplyToNewFile
        ' 
        ApplyToNewFile.Location = New Point(6, 61)
        ApplyToNewFile.Name = "ApplyToNewFile"
        ApplyToNewFile.Size = New Size(217, 29)
        ApplyToNewFile.TabIndex = 21
        ApplyToNewFile.Text = "CopyToNewVid"
        ApplyToNewFile.UseVisualStyleBackColor = True
        ' 
        ' btnSelectFolder
        ' 
        btnSelectFolder.Location = New Point(-1, 121)
        btnSelectFolder.Name = "btnSelectFolder"
        btnSelectFolder.Size = New Size(227, 29)
        btnSelectFolder.TabIndex = 22
        btnSelectFolder.Text = "Browse VideoPath"
        btnSelectFolder.UseVisualStyleBackColor = True
        ' 
        ' FFmpegPath
        ' 
        FFmpegPath.Location = New Point(232, 59)
        FFmpegPath.Name = "FFmpegPath"
        FFmpegPath.Size = New Size(764, 27)
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
        FolderPath.Location = New Point(232, 123)
        FolderPath.Name = "FolderPath"
        FolderPath.Size = New Size(764, 27)
        FolderPath.TabIndex = 1
        ' 
        ' VideoFlowPanel
        ' 
        VideoFlowPanel.Controls.Add(VideoGrid)
        VideoFlowPanel.Location = New Point(476, 161)
        VideoFlowPanel.Name = "VideoFlowPanel"
        VideoFlowPanel.Size = New Size(285, 375)
        VideoFlowPanel.TabIndex = 26
        ' 
        ' JPGOverlay
        ' 
        JPGOverlay.AutoSize = True
        JPGOverlay.Checked = True
        JPGOverlay.CheckState = CheckState.Checked
        JPGOverlay.Location = New Point(817, 339)
        JPGOverlay.Name = "JPGOverlay"
        JPGOverlay.Size = New Size(160, 24)
        JPGOverlay.TabIndex = 27
        JPGOverlay.Text = "Icon in Thumbnail ?"
        JPGOverlay.UseVisualStyleBackColor = True
        ' 
        ' IcarosPathTextbox
        ' 
        IcarosPathTextbox.Location = New Point(232, 88)
        IcarosPathTextbox.Name = "IcarosPathTextbox"
        IcarosPathTextbox.Size = New Size(764, 27)
        IcarosPathTextbox.TabIndex = 29
        ' 
        ' DLIcarosButton
        ' 
        DLIcarosButton.Location = New Point(123, 88)
        DLIcarosButton.Name = "DLIcarosButton"
        DLIcarosButton.Size = New Size(103, 29)
        DLIcarosButton.TabIndex = 28
        DLIcarosButton.Text = "DL Icaros"
        DLIcarosButton.UseVisualStyleBackColor = True
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
        CustomOverlay.Size = New Size(217, 27)
        CustomOverlay.TabIndex = 31
        CustomOverlay.Text = "JPG"
        CustomOverlay.TextAlign = HorizontalAlignment.Center
        ' 
        ' StatusBox
        ' 
        StatusBox.BackColor = SystemColors.Info
        StatusBox.Location = New Point(3, 1)
        StatusBox.Multiline = True
        StatusBox.Name = "StatusBox"
        StatusBox.Size = New Size(993, 52)
        StatusBox.TabIndex = 1
        ' 
        ' BGTextOverlay
        ' 
        BGTextOverlay.Location = New Point(73, 20)
        BGTextOverlay.Name = "BGTextOverlay"
        BGTextOverlay.Size = New Size(41, 27)
        BGTextOverlay.TabIndex = 32
        ' 
        ' FontTextOverlay
        ' 
        FontTextOverlay.Location = New Point(73, 53)
        FontTextOverlay.Name = "FontTextOverlay"
        FontTextOverlay.Size = New Size(41, 27)
        FontTextOverlay.TabIndex = 33
        ' 
        ' DLFFmpegButton
        ' 
        DLFFmpegButton.Location = New Point(123, 59)
        DLFFmpegButton.Name = "DLFFmpegButton"
        DLFFmpegButton.Size = New Size(103, 29)
        DLFFmpegButton.TabIndex = 34
        DLFFmpegButton.Text = "DL FFmpeg"
        DLFFmpegButton.UseVisualStyleBackColor = True
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
        Label5.Location = New Point(120, 56)
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
        Label2.Location = New Point(6, 53)
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
        GroupThumb.Location = New Point(9, 535)
        GroupThumb.Name = "GroupThumb"
        GroupThumb.Size = New Size(752, 202)
        GroupThumb.TabIndex = 37
        GroupThumb.TabStop = False
        GroupThumb.Text = "Thumbnails from"
        ' 
        ' VideoFileActionsGroup
        ' 
        VideoFileActionsGroup.Controls.Add(ApplyToVidFile)
        VideoFileActionsGroup.Controls.Add(ApplyToNewFile)
        VideoFileActionsGroup.Location = New Point(767, 665)
        VideoFileActionsGroup.Name = "VideoFileActionsGroup"
        VideoFileActionsGroup.Size = New Size(229, 100)
        VideoFileActionsGroup.TabIndex = 38
        VideoFileActionsGroup.TabStop = False
        VideoFileActionsGroup.Text = "VideoFile Actions"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1008, 798)
        Controls.Add(VideoFileActionsGroup)
        Controls.Add(GroupThumb)
        Controls.Add(ThumbnailEdition)
        Controls.Add(ActualFileName)
        Controls.Add(DLFFmpegButton)
        Controls.Add(StatusBox)
        Controls.Add(BrowseIcarosButton)
        Controls.Add(IcarosPathTextbox)
        Controls.Add(DLIcarosButton)
        Controls.Add(JPGOverlay)
        Controls.Add(VideoFlowPanel)
        Controls.Add(FolderPath)
        Controls.Add(BrowseFFmpeg)
        Controls.Add(FFmpegPath)
        Controls.Add(btnSelectFolder)
        Controls.Add(VideoPanel)
        Controls.Add(FinalPictureBox)
        Controls.Add(WindowsIconThumbnail)
        Name = "Form1"
        Text = "Form1"
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
        ThumbnailEdition.ResumeLayout(False)
        ThumbnailEdition.PerformLayout()
        GroupThumb.ResumeLayout(False)
        VideoFileActionsGroup.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents VideoGrid As FlowLayoutPanel
    Friend WithEvents WindowsIconThumbnail As PictureBox
    Friend WithEvents TakeScreenShot As Button
    Friend WithEvents FinalPictureBox As PictureBox
    Friend WithEvents VideoPanel As Panel
    Friend WithEvents VideoPlayer As AxWMPLib.AxWindowsMediaPlayer
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
    Friend WithEvents ApplyToNewFile As Button
    Friend WithEvents btnSelectFolder As Button
    Friend WithEvents FFmpegPath As TextBox
    Friend WithEvents BrowseFFmpeg As Button
    Friend WithEvents FolderPath As TextBox
    Friend WithEvents VideoFlowPanel As Panel
    Friend WithEvents JPGOverlay As CheckBox
    Friend WithEvents IcarosPathTextbox As TextBox
    Friend WithEvents DLIcarosButton As Button
    Friend WithEvents BrowseIcarosButton As Button
    Friend WithEvents Video As AxWMPLib.AxWindowsMediaPlayer
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

End Class
