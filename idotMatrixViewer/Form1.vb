Imports System.IO
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports WMPLib

Public Class Form1
    Private isDrawing As Boolean = False
    Private startPoint As Point
    Private currentRect As Rectangle
    Private originalScreenshot As Bitmap = Nothing

    ' Timer for drawing updates
    Private WithEvents updateTimer As New Timer With {.Interval = 50}

    ' Constants and private variables at the top of the class
    Private ReadOnly SETTINGS_FILE As String = "settings.ini"
    Private FFmpegPath As String = ""
    Private videoPath As String = ""

    ' FFmpeg path management
    Private Sub LoadFFmpegPath()
        Try
            If File.Exists(SETTINGS_FILE) Then
                FFmpegPath = File.ReadAllText(SETTINGS_FILE)
                txtFFmpegPath.Text = FFmpegPath
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading FFmpeg path: " & ex.Message)
        End Try
    End Sub

    Private Sub SaveFFmpegPath(path As String)
        Try
            File.WriteAllText(SETTINGS_FILE, path)
            FFmpegPath = path
        Catch ex As Exception
            MessageBox.Show("Error saving FFmpeg path: " & ex.Message)
        End Try
    End Sub

    Private Sub txtFFmpegPath_TextChanged(sender As Object, e As EventArgs) Handles txtFFmpegPath.TextChanged
        SaveFFmpegPath(txtFFmpegPath.Text)
    End Sub

    ' Browse and load video management
    Private Sub BtnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Video files|*.mp4;*.avi;*.mov|All files|*.*"
            If ofd.ShowDialog() = DialogResult.OK Then
                videoPath = ofd.FileName
                txtFilePath.Text = videoPath
                LoadVideo()
            End If
        End Using
    End Sub

    Private Sub LoadVideo()
        Try
            AxWindowsMediaPlayer1.Ctlcontrols.stop()
            AxWindowsMediaPlayer1.URL = videoPath
            AxWindowsMediaPlayer1.Ctlcontrols.play()

            ' Clear existing images
            If screenshotbox.Image IsNot Nothing Then
                screenshotbox.Image.Dispose()
                screenshotbox.Image = Nothing
            End If
            If Finalresultpicture.Image IsNot Nothing Then
                Finalresultpicture.Image.Dispose()
                Finalresultpicture.Image = Nothing
            End If

            ' Reset selection rectangle
            currentRect = Rectangle.Empty

            ' Update the Pause button
            btnPause.Text = "Pause"

        Catch ex As Exception
            MessageBox.Show("Error loading video: " & ex.Message)
        End Try
    End Sub

    ' Add this in Form1_Load
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initial setup
        screenshotbox.SizeMode = PictureBoxSizeMode.Zoom
        Finalresultpicture.SizeMode = PictureBoxSizeMode.Zoom

        ' Windows Media Player setup
        AxWindowsMediaPlayer1.uiMode = "full" ' Changed from "mini" to "full"
        AxWindowsMediaPlayer1.enableContextMenu = True
        AxWindowsMediaPlayer1.stretchToFit = True

        ' PictureBox setup
        screenshotbox.BackColor = Color.Transparent
        ' FinalPictureBox setup
        Finalresultpicture.SizeMode = PictureBoxSizeMode.Normal
        Finalresultpicture.BackColor = Color.Black
        Finalresultpicture.Anchor = AnchorStyles.Top Or AnchorStyles.Left

        ' Load FFmpeg path
        LoadFFmpegPath()
    End Sub

    Private Sub UpdatePreview()
        Try
            If originalScreenshot IsNot Nothing Then
                ' Create a new image from the original
                If screenshotbox.Image IsNot Nothing Then
                    screenshotbox.Image.Dispose()
                End If
                screenshotbox.Image = New Bitmap(originalScreenshot)

                ' Draw the selection rectangle
                If Not currentRect.IsEmpty Then
                    Using g As Graphics = Graphics.FromImage(screenshotbox.Image)
                        Using whitePen As New Pen(Color.White, 4)
                            g.DrawRectangle(whitePen, currentRect)
                        End Using
                        Using redPen As New Pen(Color.Red, 2)
                            g.DrawRectangle(redPen, currentRect)
                        End Using
                    End Using

                    ' Update coordinates in the textbox
                    txtCropBoxValue.Text = $"{currentRect.X};{currentRect.Y};{currentRect.Width}"
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine("Error updating preview: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
        Try
            If AxWindowsMediaPlayer1.playState = WMPPlayState.wmppsPlaying Then
                ' Pause
                AxWindowsMediaPlayer1.Ctlcontrols.pause()
                btnPause.Text = "Play"
                CaptureCurrentFrame()
            Else
                ' Resume playback
                AxWindowsMediaPlayer1.Ctlcontrols.play()
                btnPause.Text = "Pause"
                ' No longer clear images, just start real-time updates
                updateTimer.Start()
            End If
        Catch ex As Exception
            MessageBox.Show("Error pausing/resuming: " & ex.Message)
        End Try
    End Sub

    ' New method for screenshot capture
    Private Sub CaptureCurrentFrame()
        Try
            If String.IsNullOrEmpty(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
                MessageBox.Show("Invalid FFmpeg path!")
                Return
            End If

            ' Get the current video position
            Dim currentTime As Double = AxWindowsMediaPlayer1.Ctlcontrols.currentPosition
            Dim timeSpan As TimeSpan = TimeSpan.FromSeconds(currentTime)
            Dim formattedTime As String = String.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
            timeSpan.Hours,
            timeSpan.Minutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds)

            ' Create a temporary file name
            Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "temp_screenshot.png")

            ' Prepare the FFmpeg command
            Dim process As New Process()
            process.StartInfo.FileName = FFmpegPath
            process.StartInfo.Arguments = String.Format("-y -ss {0} -i ""{1}"" -vframes 1 -q:v 2 ""{2}""",
                                                  formattedTime,
                                                  videoPath,
                                                  tempImagePath)
            process.StartInfo.UseShellExecute = False
            process.StartInfo.CreateNoWindow = True
            process.StartInfo.RedirectStandardError = True

            ' Execute FFmpeg
            process.Start()
            Dim errorOutput As String = process.StandardError.ReadToEnd()
            process.WaitForExit()

            If process.ExitCode = 0 AndAlso File.Exists(tempImagePath) Then
                ' Load and resize the image
                Using loadedImage As New Bitmap(tempImagePath)
                    ' Save the original image
                    If originalScreenshot IsNot Nothing Then
                        originalScreenshot.Dispose()
                    End If
                    originalScreenshot = New Bitmap(loadedImage)

                    ' Update the screenshotbox
                    If screenshotbox.Image IsNot Nothing Then
                        screenshotbox.Image.Dispose()
                    End If
                    screenshotbox.Image = New Bitmap(loadedImage)
                End Using

                ' Delete the temporary file
                Try
                    File.Delete(tempImagePath)
                Catch ex As Exception
                    Debug.WriteLine("Error deleting temporary file: " & ex.Message)
                End Try

                updateTimer.Start()
            Else
                MessageBox.Show("Error capturing with FFmpeg: " & errorOutput)
            End If

        Catch ex As Exception
            MessageBox.Show("Error capturing: " & ex.Message)
        End Try
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If originalScreenshot IsNot Nothing Then
            originalScreenshot.Dispose()
        End If
        If screenshotbox.Image IsNot Nothing Then
            screenshotbox.Image.Dispose()
        End If
        If Finalresultpicture.Image IsNot Nothing Then
            Finalresultpicture.Image.Dispose()
        End If
    End Sub

    Private Function GetScaledPoint(mousePoint As Point) As Point
        If screenshotbox.Image Is Nothing Then Return mousePoint

        ' Calculate the scale ratio
        Dim ratioX As Double = screenshotbox.Image.Width / screenshotbox.ClientSize.Width
        Dim ratioY As Double = screenshotbox.Image.Height / screenshotbox.ClientSize.Height
        Dim ratio As Double = Math.Min(ratioX, ratioY)

        ' Calculate margins
        Dim marginLeft As Integer = (screenshotbox.ClientSize.Width - (screenshotbox.Image.Width / ratio)) / 2
        Dim marginTop As Integer = (screenshotbox.ClientSize.Height - (screenshotbox.Image.Height / ratio)) / 2

        ' Adjust coordinates
        Dim x As Integer = CInt((mousePoint.X - marginLeft) * ratio)
        Dim y As Integer = CInt((mousePoint.Y - marginTop) * ratio)

        Return New Point(x, y)
    End Function

    Private Sub ScreenshotBox_MouseDown(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseDown
        If screenshotbox.Image IsNot Nothing Then
            isDrawing = True
            startPoint = GetScaledPoint(e.Location)
            currentRect = Rectangle.Empty
            UpdatePreview()
        End If
    End Sub

    Private Sub ScreenshotBox_MouseMove(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseMove
        If isDrawing AndAlso screenshotbox.Image IsNot Nothing Then
            ' Get the scaled point
            Dim currentPoint = GetScaledPoint(e.Location)

            ' Calculate the square size (1:1 ratio)
            Dim size As Integer = Math.Min(Math.Abs(currentPoint.X - startPoint.X), Math.Abs(currentPoint.Y - startPoint.Y))
            size = Math.Max(size, 100) ' Minimum size of 100 pixels

            ' Calculate coordinates to maintain a square
            Dim x As Integer = If(currentPoint.X < startPoint.X, startPoint.X - size, startPoint.X)
            Dim y As Integer = If(currentPoint.Y < startPoint.Y, startPoint.Y - size, startPoint.Y)

            ' Ensure the square stays within the image bounds
            x = Math.Max(0, Math.Min(x, screenshotbox.Image.Width - size))
            y = Math.Max(0, Math.Min(y, screenshotbox.Image.Height - size))

            ' Update the rectangle
            currentRect = New Rectangle(x, y, size, size)

            ' Update the display
            UpdatePreview()

            ' Update coordinates in the textbox
            txtCropBoxValue.Text = $"X:{x};Y:{y};{size}"
        End If
    End Sub

    Private Sub UpdateTimer_Tick(sender As Object, e As EventArgs) Handles updateTimer.Tick
        If Not String.IsNullOrEmpty(txtCropBoxValue.Text) Then
            If AxWindowsMediaPlayer1.playState = WMPPlayState.wmppsPlaying Then
                UpdateLivePixelation()
            End If
        End If
    End Sub

    Private Sub UpdateLivePixelation()
        Try
            ' Check if we have crop values
            If String.IsNullOrEmpty(txtCropBoxValue.Text) Then Return

            ' Parse values from the textbox
            Dim values = txtCropBoxValue.Text.Split(";"c)
            If values.Length <> 3 Then Return

            Dim x As Integer, y As Integer, size As Integer
            If Not Integer.TryParse(values(0), x) OrElse
           Not Integer.TryParse(values(1), y) OrElse
           Not Integer.TryParse(values(2), size) Then Return

            ' Create a bitmap the size of the player
            Using bmp As New Bitmap(AxWindowsMediaPlayer1.Width, AxWindowsMediaPlayer1.Height)
                ' Capture the current frame
                Using g As Graphics = Graphics.FromImage(bmp)
                    AxWindowsMediaPlayer1.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
                End Using

                ' Define the crop rectangle based on textbox values
                Dim cropRect = New Rectangle(x, y, size, size)

                ' Extract the selected area
                Using cropImage As New Bitmap(size, size)
                    Using g As Graphics = Graphics.FromImage(cropImage)
                        g.DrawImage(bmp,
                              New Rectangle(0, 0, size, size),
                              cropRect,
                              GraphicsUnit.Pixel)
                    End Using

                    ' Create the pixelated version
                    Dim getpixelsize = Replace(txtPixelSize.Text, "x", ",")

                    ' Create the pixelated version
                    Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
                    Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
                    Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

                    Using pixelated As New Bitmap(pixelWidth, pixelHeight)
                        Using g As Graphics = Graphics.FromImage(pixelated)
                            g.InterpolationMode = InterpolationMode.NearestNeighbor
                            g.PixelOffsetMode = PixelOffsetMode.Half
                            g.DrawImage(cropImage, 0, 0, pixelWidth, pixelHeight)
                        End Using

                        ' Update Finalresultpicture
                        If Finalresultpicture.Image IsNot Nothing Then
                            Finalresultpicture.Image.Dispose()
                        End If
                        Finalresultpicture.Image = New Bitmap(pixelated)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Debug.WriteLine("Error updating live: " & ex.Message)
        End Try
    End Sub

    Private Sub ScreenshotBox_MouseUp(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseUp
        isDrawing = False
        If Not currentRect.IsEmpty Then
            ' Render the pixelated version of the selected area
            UpdatePixelatedImage()
        End If
    End Sub

    Private Sub UpdatePixelatedImage()
        Try
            If screenshotbox.Image IsNot Nothing AndAlso Not currentRect.IsEmpty Then
                ' Get dimensions from txtPixelSize
                Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
                Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
                Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

                ' Step 1: Reduce the image to the specified dimensions
                Using reducedImage As New Bitmap(pixelWidth, pixelHeight)
                    Using g As Graphics = Graphics.FromImage(reducedImage)
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                        g.PixelOffsetMode = PixelOffsetMode.Half

                        g.DrawImage(screenshotbox.Image,
                          New Rectangle(0, 0, pixelWidth, pixelHeight),
                          currentRect,
                          GraphicsUnit.Pixel)
                    End Using

                    ' Step 2: Enlarge the pixelated image to the size of the PictureBox
                    Using finalImage As New Bitmap(Finalresultpicture.Width, Finalresultpicture.Height)
                        Using g As Graphics = Graphics.FromImage(finalImage)
                            g.InterpolationMode = InterpolationMode.NearestNeighbor
                            g.PixelOffsetMode = PixelOffsetMode.Half

                            g.DrawImage(reducedImage,
                              0, 0, finalImage.Width, finalImage.Height)
                        End Using

                        If Finalresultpicture.Image IsNot Nothing Then
                            Finalresultpicture.Image.Dispose()
                        End If
                        Finalresultpicture.Image = finalImage.Clone()
                    End Using
                End Using
            End If
        Catch ex As Exception
            Debug.WriteLine("Error pixelating: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Try
            Dim currentTime As Double = AxWindowsMediaPlayer1.Ctlcontrols.currentPosition
            txtStartTime.Text = currentTime.ToString("0.000")
        Catch ex As Exception
            MessageBox.Show("Error capturing start time: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnEnd_Click(sender As Object, e As EventArgs) Handles btnEnd.Click
        Try
            Dim currentTime As Double = AxWindowsMediaPlayer1.Ctlcontrols.currentPosition
            txtEndTime.Text = currentTime.ToString("0.000")
        Catch ex As Exception
            MessageBox.Show("Error capturing end time: " & ex.Message)
        End Try
    End Sub

    Private Sub txtPixelSize_TextChanged(sender As Object, e As EventArgs) Handles txtPixelSize.TextChanged
        Try
            ' Validate the format
            If Not ValidatePixelSize(txtPixelSize.Text) Then
                Return
            End If

            ' If there is an active selection, update the image
            If Not currentRect.IsEmpty Then
                UpdatePixelatedImage()
            End If
        Catch ex As Exception
            Debug.WriteLine("Error updating pixel size: " & ex.Message)
        End Try
    End Sub

    Private Function ValidatePixelSize(size As String) As Boolean
        Try
            Dim dimensions() As String = size.Split("x"c)
            If dimensions.Length <> 2 Then Return False
            Dim width As Integer = Integer.Parse(dimensions(0))
            Dim height As Integer = Integer.Parse(dimensions(1))
            Return width > 0 AndAlso height > 0
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub BtnExtract_Click(sender As Object, e As EventArgs) Handles btnExtract.Click
        If String.IsNullOrEmpty(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
            MessageBox.Show("Invalid FFmpeg path!")
            Return
        End If

        Try
            Dim startTime As Double
            Dim endTime As Double

            If Not Double.TryParse(txtStartTime.Text, startTime) OrElse
           Not Double.TryParse(txtEndTime.Text, endTime) Then
                MessageBox.Show("Invalid start or end time!")
                Return
            End If

            If startTime >= endTime Then
                MessageBox.Show("Start time must be less than end time!")
                Return
            End If

            ' Clean and parse coordinates
            Dim cropValues = txtCropBoxValue.Text.Split(";")
            cropValues(0) = cropValues(0).Replace("X:", "").Trim()
            cropValues(1) = cropValues(1).Replace("Y:", "").Trim()
            cropValues(2) = cropValues(2).Trim()

            If cropValues.Length <> 3 Then
                MessageBox.Show("Invalid crop format!")
                Return
            End If

            Dim x As Integer
            Dim y As Integer
            Dim size As Integer

            If Not Integer.TryParse(cropValues(0), x) OrElse
           Not Integer.TryParse(cropValues(1), y) OrElse
           Not Integer.TryParse(cropValues(2), size) Then
                MessageBox.Show("Invalid crop values!")
                Return
            End If

            Using sfd As New SaveFileDialog()
                sfd.Filter = "GIF files|*.gif"
                If sfd.ShowDialog() = DialogResult.OK Then
                    ' Create a palette for better quality
                    Dim palettePath As String = Path.Combine(Path.GetTempPath(), "palette.png")
                    Dim getpixelsize = Replace(txtPixelSize.Text, "x", ":")
                    ' Command to generate the palette
                    Dim paletteArgs As String = String.Format(
    "-y -ss {0} -t {1} -i ""{2}"" -vf ""crop={3}:{3}:{4}:{5},palettegen"" ""{6}""",
    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
    videoPath,
    size, x, y,
    palettePath)

                    Using process As New Process()
                        process.StartInfo.FileName = FFmpegPath
                        process.StartInfo.Arguments = paletteArgs
                        process.StartInfo.UseShellExecute = False
                        process.StartInfo.CreateNoWindow = True
                        process.StartInfo.RedirectStandardError = True
                        process.Start()
                        Dim derror = process.StandardError.ReadToEnd()
                        process.WaitForExit()

                        If process.ExitCode = 0 Then
                            ' Create the final GIF with the palette
                            Dim gifArgs As String = String.Format(
    "-y -ss {0} -t {1} -i ""{2}"" -i ""{3}"" -lavfi ""crop={4}:{4}:{5}:{6} [x]; [x][1:v] paletteuse"" -f gif ""{7}""",
    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
    videoPath,
    palettePath,
    size, x, y,
    sfd.FileName)

                            Using gifProcess As New Process()
                                gifProcess.StartInfo.FileName = FFmpegPath
                                gifProcess.StartInfo.Arguments = gifArgs
                                gifProcess.StartInfo.UseShellExecute = False
                                gifProcess.StartInfo.CreateNoWindow = True
                                gifProcess.StartInfo.RedirectStandardError = True
                                gifProcess.Start()
                                Dim gifError = gifProcess.StandardError.ReadToEnd()
                                gifProcess.WaitForExit()

                                If gifProcess.ExitCode = 0 Then
                                    MessageBox.Show("GIF created successfully!")
                                Else
                                    MessageBox.Show("Error creating GIF: " & gifError)
                                End If
                            End Using
                        Else
                            MessageBox.Show("Error generating palette: " & derror)
                        End If
                    End Using

                    ' Clean up the palette file
                    If File.Exists(palettePath) Then
                        File.Delete(palettePath)
                    End If
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show("Error during extraction: " & ex.Message & vbCrLf & ex.StackTrace)
        End Try
    End Sub
End Class
