<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form remplace la méthode Dispose pour nettoyer la liste des composants.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requise par le Concepteur Windows Form
    Private components As System.ComponentModel.IContainer

    'REMARQUE : la procédure suivante est requise par le Concepteur Windows Form
    'Elle peut être modifiée à l'aide du Concepteur Windows Form.  
    'Ne la modifiez pas à l'aide de l'éditeur de code.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        txtFFmpegPath = New TextBox()
        txtFilePath = New TextBox()
        txtStartTime = New TextBox()
        txtEndTime = New TextBox()
        btnBrowse = New Button()
        btnStart = New Button()
        btnEnd = New Button()
        btnPause = New Button()
        btnResetCrop = New Button()
        btnExtract = New Button()
        Label1 = New Label()
        Finalresultpicture = New PictureBox()
        screenshotbox = New PictureBox()
        AxWindowsMediaPlayer1 = New AxWMPLib.AxWindowsMediaPlayer()
        txtCroppedX = New TextBox()
        txtPixelSize = New TextBox()
        Label2 = New Label()
        chkboomerang = New CheckBox()
        btnback = New Button()
        btnforward = New Button()
        txtsecskip = New TextBox()
        Label3 = New Label()
        txtCroppedY = New TextBox()
        txtCroppedWidth = New TextBox()
        lbllive = New Label()
        lblcoordo = New Label()
        CType(Finalresultpicture, ComponentModel.ISupportInitialize).BeginInit()
        CType(screenshotbox, ComponentModel.ISupportInitialize).BeginInit()
        CType(AxWindowsMediaPlayer1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' txtFFmpegPath
        ' 
        txtFFmpegPath.Location = New Point(134, 1)
        txtFFmpegPath.Name = "txtFFmpegPath"
        txtFFmpegPath.Size = New Size(231, 27)
        txtFFmpegPath.TabIndex = 1
        ' 
        ' txtFilePath
        ' 
        txtFilePath.Location = New Point(134, 56)
        txtFilePath.Name = "txtFilePath"
        txtFilePath.Size = New Size(231, 27)
        txtFilePath.TabIndex = 2
        ' 
        ' txtStartTime
        ' 
        txtStartTime.Location = New Point(754, 47)
        txtStartTime.Name = "txtStartTime"
        txtStartTime.Size = New Size(94, 27)
        txtStartTime.TabIndex = 3
        ' 
        ' txtEndTime
        ' 
        txtEndTime.Location = New Point(854, 47)
        txtEndTime.Name = "txtEndTime"
        txtEndTime.Size = New Size(85, 27)
        txtEndTime.TabIndex = 4
        ' 
        ' btnBrowse
        ' 
        btnBrowse.Location = New Point(8, 27)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(120, 56)
        btnBrowse.TabIndex = 5
        btnBrowse.Text = "Browse Video/Gif"
        btnBrowse.UseVisualStyleBackColor = True
        ' 
        ' btnStart
        ' 
        btnStart.Location = New Point(754, 12)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(94, 29)
        btnStart.TabIndex = 6
        btnStart.Text = "Set Start"
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' btnEnd
        ' 
        btnEnd.Location = New Point(854, 12)
        btnEnd.Name = "btnEnd"
        btnEnd.Size = New Size(85, 29)
        btnEnd.TabIndex = 7
        btnEnd.Text = "Set End"
        btnEnd.UseVisualStyleBackColor = True
        ' 
        ' btnPause
        ' 
        btnPause.Location = New Point(107, 330)
        btnPause.Name = "btnPause"
        btnPause.Size = New Size(94, 29)
        btnPause.TabIndex = 8
        btnPause.Text = "Pause"
        btnPause.UseVisualStyleBackColor = True
        ' 
        ' btnResetCrop
        ' 
        btnResetCrop.Location = New Point(456, 333)
        btnResetCrop.Name = "btnResetCrop"
        btnResetCrop.Size = New Size(93, 29)
        btnResetCrop.TabIndex = 9
        btnResetCrop.Text = "Reset Crop"
        btnResetCrop.UseVisualStyleBackColor = True
        ' 
        ' btnExtract
        ' 
        btnExtract.Location = New Point(774, 330)
        btnExtract.Name = "btnExtract"
        btnExtract.Size = New Size(94, 29)
        btnExtract.TabIndex = 10
        btnExtract.Text = "Extract Gif"
        btnExtract.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(8, 4)
        Label1.Name = "Label1"
        Label1.Size = New Size(120, 20)
        Label1.TabIndex = 11
        Label1.Text = "FFmpeg.exe Path"
        ' 
        ' Finalresultpicture
        ' 
        Finalresultpicture.Location = New Point(703, 89)
        Finalresultpicture.Name = "Finalresultpicture"
        Finalresultpicture.Size = New Size(235, 235)
        Finalresultpicture.SizeMode = PictureBoxSizeMode.Zoom
        Finalresultpicture.TabIndex = 12
        Finalresultpicture.TabStop = False
        ' 
        ' screenshotbox
        ' 
        screenshotbox.Location = New Point(331, 89)
        screenshotbox.Name = "screenshotbox"
        screenshotbox.Size = New Size(350, 191)
        screenshotbox.TabIndex = 13
        screenshotbox.TabStop = False
        ' 
        ' AxWindowsMediaPlayer1
        ' 
        AxWindowsMediaPlayer1.Enabled = True
        AxWindowsMediaPlayer1.Location = New Point(13, 89)
        AxWindowsMediaPlayer1.Name = "AxWindowsMediaPlayer1"
        AxWindowsMediaPlayer1.OcxState = CType(resources.GetObject("AxWindowsMediaPlayer1.OcxState"), AxHost.State)
        AxWindowsMediaPlayer1.Size = New Size(298, 235)
        AxWindowsMediaPlayer1.TabIndex = 14
        ' 
        ' txtCroppedX
        ' 
        txtCroppedX.Location = New Point(530, 290)
        txtCroppedX.Name = "txtCroppedX"
        txtCroppedX.Size = New Size(47, 27)
        txtCroppedX.TabIndex = 15
        txtCroppedX.Text = "0"
        txtCroppedX.TextAlign = HorizontalAlignment.Center
        ' 
        ' txtPixelSize
        ' 
        txtPixelSize.Location = New Point(675, 47)
        txtPixelSize.Name = "txtPixelSize"
        txtPixelSize.Size = New Size(73, 27)
        txtPixelSize.TabIndex = 16
        txtPixelSize.Text = "64x64"
        txtPixelSize.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(675, 16)
        Label2.Name = "Label2"
        Label2.Size = New Size(67, 20)
        Label2.TabIndex = 17
        Label2.Text = "PixelSize"
        ' 
        ' chkboomerang
        ' 
        chkboomerang.AutoSize = True
        chkboomerang.Location = New Point(331, 328)
        chkboomerang.Name = "chkboomerang"
        chkboomerang.Size = New Size(109, 24)
        chkboomerang.TabIndex = 18
        chkboomerang.Text = "Boomerang"
        chkboomerang.UseVisualStyleBackColor = True
        ' 
        ' btnback
        ' 
        btnback.Location = New Point(12, 328)
        btnback.Name = "btnback"
        btnback.Size = New Size(94, 29)
        btnback.TabIndex = 19
        btnback.Text = "<<"
        btnback.UseVisualStyleBackColor = True
        ' 
        ' btnforward
        ' 
        btnforward.Location = New Point(217, 328)
        btnforward.Name = "btnforward"
        btnforward.Size = New Size(94, 29)
        btnforward.TabIndex = 20
        btnforward.Text = ">>"
        btnforward.UseVisualStyleBackColor = True
        ' 
        ' txtsecskip
        ' 
        txtsecskip.Location = New Point(331, 286)
        txtsecskip.Name = "txtsecskip"
        txtsecskip.Size = New Size(42, 27)
        txtsecskip.TabIndex = 21
        txtsecskip.Text = "0,5"
        txtsecskip.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(379, 290)
        Label3.Name = "Label3"
        Label3.Size = New Size(60, 20)
        Label3.TabIndex = 22
        Label3.Text = "sec skip"
        ' 
        ' txtCroppedY
        ' 
        txtCroppedY.Location = New Point(583, 290)
        txtCroppedY.Name = "txtCroppedY"
        txtCroppedY.Size = New Size(39, 27)
        txtCroppedY.TabIndex = 23
        txtCroppedY.Text = "0"
        txtCroppedY.TextAlign = HorizontalAlignment.Center
        ' 
        ' txtCroppedWidth
        ' 
        txtCroppedWidth.Location = New Point(628, 290)
        txtCroppedWidth.Name = "txtCroppedWidth"
        txtCroppedWidth.Size = New Size(53, 27)
        txtCroppedWidth.TabIndex = 24
        txtCroppedWidth.Text = "0"
        txtCroppedWidth.TextAlign = HorizontalAlignment.Center
        ' 
        ' lbllive
        ' 
        lbllive.AutoSize = True
        lbllive.Location = New Point(590, 320)
        lbllive.Name = "lbllive"
        lbllive.Size = New Size(91, 20)
        lbllive.TabIndex = 25
        lbllive.Text = "X[#;#];Y[#;#]"
        ' 
        ' lblcoordo
        ' 
        lblcoordo.AutoSize = True
        lblcoordo.Location = New Point(590, 343)
        lblcoordo.Name = "lblcoordo"
        lblcoordo.Size = New Size(91, 20)
        lblcoordo.TabIndex = 26
        lblcoordo.Text = "X[#;#];Y[#;#]"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(953, 372)
        Controls.Add(lblcoordo)
        Controls.Add(lbllive)
        Controls.Add(txtCroppedWidth)
        Controls.Add(txtCroppedY)
        Controls.Add(Label3)
        Controls.Add(txtsecskip)
        Controls.Add(btnforward)
        Controls.Add(btnback)
        Controls.Add(chkboomerang)
        Controls.Add(Label2)
        Controls.Add(txtPixelSize)
        Controls.Add(txtCroppedX)
        Controls.Add(Finalresultpicture)
        Controls.Add(screenshotbox)
        Controls.Add(AxWindowsMediaPlayer1)
        Controls.Add(Label1)
        Controls.Add(btnExtract)
        Controls.Add(btnResetCrop)
        Controls.Add(btnPause)
        Controls.Add(btnEnd)
        Controls.Add(btnStart)
        Controls.Add(btnBrowse)
        Controls.Add(txtEndTime)
        Controls.Add(txtStartTime)
        Controls.Add(txtFilePath)
        Controls.Add(txtFFmpegPath)
        Name = "Form1"
        Text = "Form1"
        CType(Finalresultpicture, ComponentModel.ISupportInitialize).EndInit()
        CType(screenshotbox, ComponentModel.ISupportInitialize).EndInit()
        CType(AxWindowsMediaPlayer1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
    Friend WithEvents txtFFmpegPath As TextBox
    Friend WithEvents txtFilePath As TextBox
    Friend WithEvents txtStartTime As TextBox
    Friend WithEvents txtEndTime As TextBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents btnStart As Button
    Friend WithEvents btnEnd As Button
    Friend WithEvents btnPause As Button
    Friend WithEvents btnResetCrop As Button
    Friend WithEvents btnExtract As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents Finalresultpicture As PictureBox
    Friend WithEvents screenshotbox As PictureBox
    Friend WithEvents AxWindowsMediaPlayer1 As AxWMPLib.AxWindowsMediaPlayer
    Friend WithEvents txtCroppedX As TextBox
    Friend WithEvents txtPixelSize As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents chkboomerang As CheckBox
    Friend WithEvents btnback As Button
    Friend WithEvents btnforward As Button
    Friend WithEvents txtsecskip As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents gifAnimationTimer As Timer
    Friend WithEvents txtCroppedY As TextBox
    Friend WithEvents txtCroppedWidth As TextBox
    Friend WithEvents lbllive As Label
    Friend WithEvents lblcoordo As Label
End Class
