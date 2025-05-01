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
            ofd.Filter = "Video files|*.wmv;*.mp4;*.avi;*.mov|All files|*.*"
            If ofd.ShowDialog() = DialogResult.OK Then
                videoPath = ofd.FileName
                txtFilePath.Text = videoPath
                LoadVideo()
            End If
        End Using
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
    Private Sub UpdateTimer_Tick(sender As Object, e As EventArgs) Handles updateTimer.Tick
        If Not String.IsNullOrEmpty(txtCroppedX.Text) Then
            If AxWindowsMediaPlayer1.playState = WMPPlayState.wmppsPlaying Then
                UpdateLivePixelation()
            End If
        End If
    End Sub

    Private Sub UpdateLivePixelation()
        Try
            ' Check if we have crop values
            If String.IsNullOrEmpty(txtCroppedX.Text) Then Return

            ' Parse values from the textbox
            Dim values = txtCroppedX.Text.Split(";"c)
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

    Private Sub BtnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Try
            Dim currentTime As Double = AxWindowsMediaPlayer1.Ctlcontrols.currentPosition
            txtStartTime.Text = currentTime.ToString("0.000")

            ' Vérifier si txtStartTime n'est pas vide ET txtEndTime est vide
            If Not String.IsNullOrEmpty(txtStartTime.Text) AndAlso String.IsNullOrEmpty(txtEndTime.Text) Then
                ' Pause la vidéo
                AxWindowsMediaPlayer1.Ctlcontrols.pause()
                btnPause.Text = "Play"

                ' Prendre un screenshot et l'afficher
                CaptureCurrentFrame()
            End If
        Catch ex As Exception
            MessageBox.Show("Error capturing start time: " & ex.Message)
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

    ' Mise à jour de la méthode UpdatePreview pour mettre à jour txtCropBoxValue
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
                    lbllive.Text = $"X:{currentRect.X};Y:{currentRect.Y};W:{currentRect.Width}"
                End If
            End If
        Catch ex As Exception
            Debug.WriteLine("Error updating preview: " & ex.Message)
        End Try
    End Sub

    ' Ajout d'une méthode pour réinitialiser les labels quand on charge une nouvelle vidéo
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

            ' Reset coordinate labels
            lblLive.Text = "[X;Y]"
            lblCoordo.Text = "X[#;#];Y[#;#]"

            ' Update the Pause button
            btnPause.Text = "Pause"

        Catch ex As Exception
            MessageBox.Show("Error loading video: " & ex.Message)
        End Try
    End Sub

    ' Ajout d'une méthode pour réinitialiser les labels quand on affiche une image noire
    Private Sub DisplayBlackImage()
        Try
            ' Nettoyer l'image existante
            If screenshotbox.Image IsNot Nothing Then
                screenshotbox.Image.Dispose()
            End If

            ' Créer une image noire de la taille du screenshotbox
            Dim blackImage As New Bitmap(screenshotbox.Width, screenshotbox.Height)
            Using g As Graphics = Graphics.FromImage(blackImage)
                g.Clear(Color.Black)
            End Using

            ' Afficher l'image noire
            screenshotbox.Image = blackImage

            ' Réinitialiser originalScreenshot
            If originalScreenshot IsNot Nothing Then
                originalScreenshot.Dispose()
            End If
            originalScreenshot = New Bitmap(blackImage)

            ' Réinitialiser les labels de coordonnées
            lblLive.Text = "[X;Y]"
            lblCoordo.Text = "X[#;#];Y[#;#]"

            ' Réinitialiser le rectangle de sélection
            currentRect = Rectangle.Empty
        Catch ex As Exception
            Debug.WriteLine("Error displaying black image: " & ex.Message)
        End Try
    End Sub

    ' Ajout d'une méthode pour gérer le MouseLeave de screenshotbox
    Private Sub ScreenshotBox_MouseLeave(sender As Object, e As EventArgs) Handles screenshotbox.MouseLeave
        ' Réinitialiser le label "live" quand la souris quitte l'image
        If Not isDrawing Then
            lblLive.Text = "[X;Y]"
        End If
    End Sub











    Private Sub GenerateAndDisplayPreviewGif(startTime As Double, endTime As Double)
        If String.IsNullOrEmpty(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
            MessageBox.Show("Invalid FFmpeg path!")
            Return
        End If

        Try
            ' Créer des fichiers temporaires
            Dim tempGifPath As String = Path.Combine(Path.GetTempPath(), "temp_preview.gif")
            Dim palettePath As String = Path.Combine(Path.GetTempPath(), "temp_palette.png")

            ' Étape 1: Générer une palette optimisée pour cette séquence vidéo
            Using paletteProcess As New Process()
                paletteProcess.StartInfo.FileName = FFmpegPath
                paletteProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -vf ""fps=15,scale=320:-1:flags=lanczos,palettegen=stats_mode=diff"" ""{3}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    palettePath)
                paletteProcess.StartInfo.UseShellExecute = False
                paletteProcess.StartInfo.CreateNoWindow = True
                paletteProcess.StartInfo.RedirectStandardError = True
                paletteProcess.Start()
                Dim paletteError As String = paletteProcess.StandardError.ReadToEnd()
                paletteProcess.WaitForExit()

                If paletteProcess.ExitCode <> 0 OrElse Not File.Exists(palettePath) Then
                    MessageBox.Show("Error generating palette: " & paletteError)
                    DisplayBlackImage()
                    Return
                End If
            End Using

            ' Étape 2: Utiliser la palette pour générer un GIF de haute qualité
            Using gifProcess As New Process()
                gifProcess.StartInfo.FileName = FFmpegPath
                gifProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -i ""{3}"" -lavfi ""fps=15,scale=320:-1:flags=lanczos[x];[x][1:v]paletteuse=dither=sierra2_4a"" ""{4}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    palettePath,
                    tempGifPath)
                gifProcess.StartInfo.UseShellExecute = False
                gifProcess.StartInfo.CreateNoWindow = True
                gifProcess.StartInfo.RedirectStandardError = True
                gifProcess.Start()
                Dim gifError As String = gifProcess.StandardError.ReadToEnd()
                gifProcess.WaitForExit()

                If gifProcess.ExitCode = 0 AndAlso File.Exists(tempGifPath) Then
                    ' Charger le GIF dans le screenshotbox
                    If screenshotbox.Image IsNot Nothing Then
                        screenshotbox.Image.Dispose()
                    End If

                    ' Utiliser Image.FromFile pour charger le GIF animé
                    screenshotbox.Image = Image.FromFile(tempGifPath)

                    ' Capturer également la première frame pour originalScreenshot
                    Using gifImage As New Bitmap(tempGifPath)
                        If originalScreenshot IsNot Nothing Then
                            originalScreenshot.Dispose()
                        End If
                        originalScreenshot = New Bitmap(gifImage)
                    End Using

                    ' Nettoyer le fichier de palette
                    Try
                        If File.Exists(palettePath) Then
                            File.Delete(palettePath)
                        End If
                    Catch ex As Exception
                        Debug.WriteLine("Error deleting palette file: " & ex.Message)
                    End Try
                Else
                    MessageBox.Show("Error generating preview: " & gifError)
                    DisplayBlackImage()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error generating preview: " & ex.Message)
            DisplayBlackImage()
        End Try
    End Sub








    ' Mise à jour de la méthode ScreenshotBox_MouseDown pour ne pas interrompre l'animation
    Private Sub ScreenshotBox_MouseDown(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseDown
        If screenshotbox.Image IsNot Nothing Then
            isDrawing = True
            startPoint = GetScaledPoint(e.Location)
            currentRect = Rectangle.Empty

            ' Initialiser le label "coordo" avec les coordonnées de départ
            lblcoordo.Text = $"X[{startPoint.X};{startPoint.X}];Y[{startPoint.Y};{startPoint.Y}]"

            ' Ne pas appeler UpdatePreview() ici pour ne pas remplacer l'image GIF animée
            ' Nous allons dessiner le rectangle directement sur le contrôle
        End If
    End Sub

    ' Mise à jour de la méthode ScreenshotBox_MouseMove pour dessiner le rectangle sans remplacer l'image
    Private Sub ScreenshotBox_MouseMove(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseMove
        If screenshotbox.Image IsNot Nothing Then
            ' Obtenir les coordonnées mises à l'échelle
            Dim scaledPoint = GetScaledPoint(e.Location)

            ' Mettre à jour le label "live" avec les coordonnées actuelles
            lbllive.Text = $"[{scaledPoint.X};{scaledPoint.Y}]"

            ' Si on est en train de dessiner, mettre à jour le rectangle et les coordonnées
            If isDrawing Then
                ' Get the scaled point
                Dim currentPoint = scaledPoint

                ' Calculate the square size (1:1 ratio)
                Dim size As Integer = Math.Min(Math.Abs(currentPoint.X - startPoint.X), Math.Abs(currentPoint.Y - startPoint.Y))
                size = Math.Max(size, 10) ' Minimum size of 10 pixels

                ' Calculate coordinates to maintain a square
                Dim x As Integer = If(currentPoint.X < startPoint.X, startPoint.X - size, startPoint.X)
                Dim y As Integer = If(currentPoint.Y < startPoint.Y, startPoint.Y - size, startPoint.Y)

                ' Ensure the square stays within the image bounds
                x = Math.Max(0, Math.Min(x, screenshotbox.Image.Width - size))
                y = Math.Max(0, Math.Min(y, screenshotbox.Image.Height - size))

                ' Update the rectangle
                currentRect = New Rectangle(x, y, size, size)

                ' Calculer les coordonnées min/max
                Dim xMin As Integer = x
                Dim yMin As Integer = y
                Dim xMax As Integer = x + size
                Dim yMax As Integer = y + size

                ' Mettre à jour le label "coordo" avec les coordonnées min/max
                lblcoordo.Text = $"X[{xMin};{xMax}];Y[{yMin};{yMax}]"

                ' Update coordinates in the textbox
                txtCroppedX.Text = x.ToString()
                txtCroppedY.Text = y.ToString()
                txtCroppedWidth.Text = size.ToString()

                ' Forcer le rafraîchissement du contrôle pour redessiner le rectangle
                screenshotbox.Invalidate()
            End If
        End If
    End Sub

    ' Ajouter un gestionnaire d'événement Paint pour dessiner le rectangle par-dessus l'image
    Private Sub ScreenshotBox_Paint(sender As Object, e As PaintEventArgs) Handles screenshotbox.Paint
        If Not currentRect.IsEmpty AndAlso screenshotbox.Image IsNot Nothing Then
            ' Convertir les coordonnées de l'image en coordonnées du contrôle
            Dim displayRect As Rectangle = GetDisplayRectangle(currentRect)

            ' Dessiner le rectangle avec un contour blanc et rouge
            Using whitePen As New Pen(Color.White, 4)
                e.Graphics.DrawRectangle(whitePen, displayRect)
            End Using
            Using redPen As New Pen(Color.Red, 2)
                e.Graphics.DrawRectangle(redPen, displayRect)
            End Using
        End If
    End Sub

    ' Ajouter une méthode pour convertir les coordonnées de l'image en coordonnées du contrôle
    Private Function GetDisplayRectangle(imageRect As Rectangle) As Rectangle
        If screenshotbox.Image Is Nothing Then Return Rectangle.Empty

        ' Calculer le ratio d'affichage
        Dim ratioX As Double = screenshotbox.ClientSize.Width / CSng(screenshotbox.Image.Width)
        Dim ratioY As Double = screenshotbox.ClientSize.Height / CSng(screenshotbox.Image.Height)
        Dim ratio As Double = Math.Min(ratioX, ratioY)

        ' Calculer les marges
        Dim marginLeft As Integer = (screenshotbox.ClientSize.Width - (screenshotbox.Image.Width * ratio)) / 2
        Dim marginTop As Integer = (screenshotbox.ClientSize.Height - (screenshotbox.Image.Height * ratio)) / 2

        ' Convertir les coordonnées
        Dim x As Integer = CInt(imageRect.X * ratio) + marginLeft
        Dim y As Integer = CInt(imageRect.Y * ratio) + marginTop
        Dim width As Integer = CInt(imageRect.Width * ratio)
        Dim height As Integer = CInt(imageRect.Height * ratio)

        Return New Rectangle(x, y, width, height)
    End Function

    ' Mise à jour de la méthode MouseUp pour générer un GIF de la zone sélectionnée
    Private Sub ScreenshotBox_MouseUp(sender As Object, e As MouseEventArgs) Handles screenshotbox.MouseUp
        If isDrawing AndAlso Not currentRect.IsEmpty Then
            isDrawing = False

            ' Calculer les coordonnées min/max finales
            Dim xMin As Integer = currentRect.X
            Dim yMin As Integer = currentRect.Y
            Dim xMax As Integer = currentRect.X + currentRect.Width
            Dim yMax As Integer = currentRect.Y + currentRect.Height

            ' Mettre à jour le label "coordo" avec les coordonnées finales
            lblcoordo.Text = $"X[{xMin};{xMax}];Y[{yMin};{yMax}]"

            ' Mettre à jour txtCropBoxValue avec les valeurs finales
            txtCroppedX.Text = currentRect.X.ToString()
            txtCroppedY.Text = currentRect.Y.ToString()
            txtCroppedWidth.Text = currentRect.Width.ToString()

            ' Mettre à jour l'aperçu pixelisé sans modifier l'image source
            UpdatePixelatedImage()

            ' Forcer le rafraîchissement du contrôle pour redessiner le rectangle
            screenshotbox.Invalidate()

            ' Générer un GIF de la zone sélectionnée si nous avons des timecodes
            If Not String.IsNullOrEmpty(txtStartTime.Text) AndAlso Not String.IsNullOrEmpty(txtEndTime.Text) Then
                Dim startTime As Double
                Dim endTime As Double

                If Double.TryParse(txtStartTime.Text, startTime) AndAlso Double.TryParse(txtEndTime.Text, endTime) Then
                    If endTime > startTime Then
                        ' Générer un GIF de la zone sélectionnée
                        GenerateSelectedAreaGif(startTime, endTime, currentRect.X, currentRect.Y, currentRect.Width)
                    End If
                End If
            End If
        End If

        isDrawing = False
    End Sub


    ' Méthode pour effacer l'image dans Finalresultpicture
    Private Sub ClearFinalResultPicture()
        If Finalresultpicture.Image IsNot Nothing Then
            Finalresultpicture.Image.Dispose()
            Finalresultpicture.Image = Nothing
        End If
    End Sub

    ' Mise à jour de la méthode Form1_Load pour configurer correctement Finalresultpicture
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initial setup
        screenshotbox.SizeMode = PictureBoxSizeMode.Zoom
        Finalresultpicture.SizeMode = PictureBoxSizeMode.StretchImage  ' Changé à StretchImage

        ' Windows Media Player setup
        AxWindowsMediaPlayer1.uiMode = "full"
        AxWindowsMediaPlayer1.enableContextMenu = True
        AxWindowsMediaPlayer1.stretchToFit = True

        ' PictureBox setup
        screenshotbox.BackColor = Color.Transparent

        ' FinalPictureBox setup
        Finalresultpicture.BackColor = Color.Black
        Finalresultpicture.Anchor = AnchorStyles.Top Or AnchorStyles.Left

        ' Load FFmpeg path
        LoadFFmpegPath()
    End Sub

    ' Correction de la méthode BtnEnd_Click pour s'assurer que le GIF s'affiche dans l'output
    Private Sub BtnEnd_Click(sender As Object, e As EventArgs) Handles btnEnd.Click
        Try
            Dim currentTime As Double = AxWindowsMediaPlayer1.Ctlcontrols.currentPosition
            txtEndTime.Text = currentTime.ToString("0.000")

            ' Cas 1: txtStartTime est vide - afficher une image noire
            If String.IsNullOrEmpty(txtStartTime.Text) Then
                DisplayBlackImage()
                ClearFinalResultPicture()
                Return
            End If

            ' Cas 2: txtStartTime ET txtEndTime ne sont pas vides
            If Not String.IsNullOrEmpty(txtStartTime.Text) AndAlso Not String.IsNullOrEmpty(txtEndTime.Text) Then
                Dim startTime As Double
                Dim endTime As Double

                If Double.TryParse(txtStartTime.Text, startTime) AndAlso Double.TryParse(txtEndTime.Text, endTime) Then
                    ' Vérifier si endTime > startTime
                    If endTime > startTime Then
                        ' Générer et afficher le GIF temporaire dans screenshotbox
                        GenerateAndDisplayPreviewGif(startTime, endTime)

                        ' Générer également un GIF carré pour le output
                        ' Utiliser un délai pour s'assurer que le GIF dans screenshotbox est chargé d'abord
                        Dim timer As New Timer()
                        timer.Interval = 500
                        AddHandler timer.Tick, Sub(s, args)
                                                   timer.Stop()
                                                   GenerateSquareOutputGif(startTime, endTime)
                                               End Sub
                        timer.Start()
                    Else
                        ' Si endTime <= startTime, afficher une image noire
                        DisplayBlackImage()
                        ClearFinalResultPicture()
                    End If
                Else
                    ' Si les valeurs ne sont pas des nombres valides
                    DisplayBlackImage()
                    ClearFinalResultPicture()
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error capturing end time: " & ex.Message)
        End Try
    End Sub

    ' Correction de la méthode GenerateSquareOutputGif pour utiliser les coordonnées correctes
    Private Sub GenerateSquareOutputGif(startTime As Double, endTime As Double)
        If String.IsNullOrEmpty(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
            Debug.WriteLine("Invalid FFmpeg path!")
            Return
        End If

        Try
            ' Utiliser directement les valeurs des textbox pour les coordonnées
            Dim x As Integer, y As Integer, size As Integer

            If Not Integer.TryParse(txtCroppedX.Text, x) OrElse
               Not Integer.TryParse(txtCroppedY.Text, y) OrElse
               Not Integer.TryParse(txtCroppedWidth.Text, size) Then
                Debug.WriteLine("Invalid crop values!")
                Return
            End If

            ' Obtenir les dimensions de pixelisation
            Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
            Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
            Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

            ' Créer des fichiers temporaires
            Dim tempOutputGifPath As String = Path.Combine(Path.GetTempPath(), "temp_square_output.gif")
            Dim palettePath As String = Path.Combine(Path.GetTempPath(), "temp_square_palette.png")

            ' Étape 1: Générer une palette optimisée
            Using paletteProcess As New Process()
                paletteProcess.StartInfo.FileName = FFmpegPath
                paletteProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -vf ""crop={3}:{3}:{4}:{5},fps=15,scale={6}:{7}:flags=lanczos,palettegen=stats_mode=diff"" ""{8}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    size, x, y,
                    pixelWidth, pixelHeight,
                    palettePath)
                paletteProcess.StartInfo.UseShellExecute = False
                paletteProcess.StartInfo.CreateNoWindow = True
                paletteProcess.StartInfo.RedirectStandardError = True
                paletteProcess.Start()
                Dim paletteError As String = paletteProcess.StandardError.ReadToEnd()
                paletteProcess.WaitForExit()

                If paletteProcess.ExitCode <> 0 OrElse Not File.Exists(palettePath) Then
                    Debug.WriteLine("Error generating square palette: " & paletteError)
                    Return
                End If
            End Using

            ' Étape 2: Utiliser la palette pour générer un GIF carré
            Using gifProcess As New Process()
                gifProcess.StartInfo.FileName = FFmpegPath
                gifProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -i ""{3}"" -lavfi ""crop={4}:{4}:{5}:{6},fps=15,scale={7}:{8}:flags=lanczos [x]; [x][1:v] paletteuse=dither=sierra2_4a"" -f gif ""{9}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    palettePath,
                    size, x, y,
                    pixelWidth, pixelHeight,
                    tempOutputGifPath)
                gifProcess.StartInfo.UseShellExecute = False
                gifProcess.StartInfo.CreateNoWindow = True
                gifProcess.StartInfo.RedirectStandardError = True
                gifProcess.Start()
                Dim gifError As String = gifProcess.StandardError.ReadToEnd()
                gifProcess.WaitForExit()

                If gifProcess.ExitCode = 0 AndAlso File.Exists(tempOutputGifPath) Then
                    ' Charger le GIF dans le Finalresultpicture
                    If Finalresultpicture.Image IsNot Nothing Then
                        Finalresultpicture.Image.Dispose()
                    End If

                    ' Utiliser Image.FromFile pour charger le GIF animé
                    Finalresultpicture.Image = Image.FromFile(tempOutputGifPath)

                    ' Configurer le PictureBox pour étirer l'image
                    Finalresultpicture.SizeMode = PictureBoxSizeMode.StretchImage

                    ' Afficher un message de débogage pour confirmer que le GIF a été généré
                    Debug.WriteLine("Output GIF generated successfully")
                Else
                    Debug.WriteLine("Error generating square output GIF: " & gifError)
                    ClearFinalResultPicture()
                End If
            End Using

            ' Nettoyer le fichier de palette
            Try
                If File.Exists(palettePath) Then
                    File.Delete(palettePath)
                End If
            Catch ex As Exception
                Debug.WriteLine("Error deleting palette file: " & ex.Message)
            End Try
        Catch ex As Exception
            Debug.WriteLine("Error generating square output GIF: " & ex.Message)
            ClearFinalResultPicture()
        End Try
    End Sub

    ' Correction de la méthode GenerateSelectedAreaGif pour utiliser les coordonnées correctes
    Private Sub GenerateSelectedAreaGif(startTime As Double, endTime As Double, x As Integer, y As Integer, size As Integer)
        If String.IsNullOrEmpty(FFmpegPath) OrElse Not File.Exists(FFmpegPath) Then
            Debug.WriteLine("Invalid FFmpeg path!")
            Return
        End If

        Try
            ' Obtenir les dimensions de pixelisation
            Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
            Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
            Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

            ' Créer des fichiers temporaires
            Dim tempOutputGifPath As String = Path.Combine(Path.GetTempPath(), "temp_output.gif")
            Dim palettePath As String = Path.Combine(Path.GetTempPath(), "temp_output_palette.png")

            ' Étape 1: Générer une palette optimisée pour cette séquence vidéo et cette zone
            Using paletteProcess As New Process()
                paletteProcess.StartInfo.FileName = FFmpegPath
                paletteProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -vf ""crop={3}:{3}:{4}:{5},fps=15,scale={6}:{7}:flags=lanczos,palettegen=stats_mode=diff"" ""{8}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    size, x, y,
                    pixelWidth, pixelHeight,
                    palettePath)
                paletteProcess.StartInfo.UseShellExecute = False
                paletteProcess.StartInfo.CreateNoWindow = True
                paletteProcess.StartInfo.RedirectStandardError = True
                paletteProcess.Start()
                Dim paletteError As String = paletteProcess.StandardError.ReadToEnd()
                paletteProcess.WaitForExit()

                If paletteProcess.ExitCode <> 0 OrElse Not File.Exists(palettePath) Then
                    Debug.WriteLine("Error generating palette: " & paletteError)
                    Return
                End If
            End Using

            ' Étape 2: Utiliser la palette pour générer un GIF de haute qualité
            Using gifProcess As New Process()
                gifProcess.StartInfo.FileName = FFmpegPath
                gifProcess.StartInfo.Arguments = String.Format(
                    "-y -ss {0} -t {1} -i ""{2}"" -i ""{3}"" -lavfi ""crop={4}:{4}:{5}:{6},fps=15,scale={7}:{8}:flags=lanczos [x]; [x][1:v] paletteuse=dither=sierra2_4a"" -f gif ""{9}""",
                    startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    videoPath,
                    palettePath,
                    size, x, y,
                    pixelWidth, pixelHeight,
                    tempOutputGifPath)
                gifProcess.StartInfo.UseShellExecute = False
                gifProcess.StartInfo.CreateNoWindow = True
                gifProcess.StartInfo.RedirectStandardError = True
                gifProcess.Start()
                Dim gifError As String = gifProcess.StandardError.ReadToEnd()
                gifProcess.WaitForExit()

                If gifProcess.ExitCode = 0 AndAlso File.Exists(tempOutputGifPath) Then
                    ' Charger le GIF dans le Finalresultpicture
                    If Finalresultpicture.Image IsNot Nothing Then
                        Finalresultpicture.Image.Dispose()
                    End If

                    ' Utiliser Image.FromFile pour charger le GIF animé
                    Finalresultpicture.Image = Image.FromFile(tempOutputGifPath)

                    ' S'assurer que le mode d'affichage est StretchImage
                    Finalresultpicture.SizeMode = PictureBoxSizeMode.StretchImage

                    ' Afficher un message de débogage pour confirmer que le GIF a été généré
                    Debug.WriteLine("Selected area GIF generated successfully")
                Else
                    Debug.WriteLine("Error generating output GIF: " & gifError)
                End If
            End Using

            ' Nettoyer le fichier de palette
            Try
                If File.Exists(palettePath) Then
                    File.Delete(palettePath)
                End If
            Catch ex As Exception
                Debug.WriteLine("Error deleting palette file: " & ex.Message)
            End Try
        Catch ex As Exception
            Debug.WriteLine("Error generating output GIF: " & ex.Message)
        End Try
    End Sub

    ' Correction de la méthode BtnExtract_Click pour utiliser les coordonnées correctes
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

            ' Récupérer les coordonnées du rectangle
            Dim x As Integer, y As Integer, size As Integer

            If Not Integer.TryParse(txtCroppedX.Text, x) OrElse
               Not Integer.TryParse(txtCroppedY.Text, y) OrElse
               Not Integer.TryParse(txtCroppedWidth.Text, size) Then
                MessageBox.Show("Invalid crop values!")
                Return
            End If

            Using sfd As New SaveFileDialog()
                sfd.Filter = "GIF files|*.gif"
                If sfd.ShowDialog() = DialogResult.OK Then
                    ' Create a palette for better quality
                    Dim palettePath As String = Path.Combine(Path.GetTempPath(), "palette.png")

                    ' Obtenir les dimensions de pixelisation
                    Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
                    Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
                    Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

                    ' Command to generate the palette - amélioration de la qualité
                    Dim paletteArgs As String = String.Format(
                        "-y -ss {0} -t {1} -i ""{2}"" -vf ""crop={3}:{3}:{4}:{5},fps=15,scale={6}:{7}:flags=lanczos,palettegen=stats_mode=diff"" ""{8}""",
                        startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        videoPath,
                        size, x, y,
                        pixelWidth, pixelHeight,
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
                            ' Create the final GIF with the palette - amélioration de la qualité
                            Dim gifArgs As String = String.Format(
                                "-y -ss {0} -t {1} -i ""{2}"" -i ""{3}"" -lavfi ""crop={4}:{4}:{5}:{6},fps=15,scale={7}:{8}:flags=lanczos [x]; [x][1:v] paletteuse=dither=sierra2_4a"" -f gif ""{9}""",
                                startTime.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                (endTime - startTime).ToString(System.Globalization.CultureInfo.InvariantCulture),
                                videoPath,
                                palettePath,
                                size, x, y,
                                pixelWidth, pixelHeight,
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

    ' Correction de la méthode UpdatePixelatedImage pour utiliser les coordonnées correctes
    Private Sub UpdatePixelatedImage()
        Try
            If screenshotbox.Image IsNot Nothing AndAlso Not currentRect.IsEmpty Then
                ' Utiliser directement les valeurs des textbox pour les coordonnées
                Dim x As Integer, y As Integer, size As Integer

                If Not Integer.TryParse(txtCroppedX.Text, x) OrElse
                   Not Integer.TryParse(txtCroppedY.Text, y) OrElse
                   Not Integer.TryParse(txtCroppedWidth.Text, size) Then
                    Debug.WriteLine("Invalid crop values!")
                    Return
                End If

                ' Créer une copie de l'image originale pour extraire la zone sélectionnée
                Dim tempImage As New Bitmap(originalScreenshot)

                ' Extraire la zone sélectionnée
                Dim cropRect = New Rectangle(x, y, size, size)
                Dim cropImage As New Bitmap(size, size)
                Using g As Graphics = Graphics.FromImage(cropImage)
                    g.DrawImage(tempImage,
                               New Rectangle(0, 0, cropImage.Width, cropImage.Height),
                               cropRect,
                               GraphicsUnit.Pixel)
                End Using

                ' Get dimensions from txtPixelSize
                Dim dimensions() As String = txtPixelSize.Text.Split("x"c)
                Dim pixelWidth As Integer = Integer.Parse(dimensions(0))
                Dim pixelHeight As Integer = Integer.Parse(dimensions(1))

                ' Réduire l'image aux dimensions spécifiées
                Using reducedImage As New Bitmap(pixelWidth, pixelHeight)
                    Using g As Graphics = Graphics.FromImage(reducedImage)
                        g.InterpolationMode = InterpolationMode.NearestNeighbor
                        g.PixelOffsetMode = PixelOffsetMode.Half
                        g.DrawImage(cropImage, 0, 0, pixelWidth, pixelHeight)
                    End Using

                    ' Utiliser directement l'image réduite
                    If Finalresultpicture.Image IsNot Nothing Then
                        Finalresultpicture.Image.Dispose()
                    End If
                    Finalresultpicture.Image = reducedImage.Clone()

                    ' S'assurer que le mode d'affichage est StretchImage
                    Finalresultpicture.SizeMode = PictureBoxSizeMode.StretchImage
                End Using

                ' Nettoyer
                tempImage.Dispose()
                cropImage.Dispose()
            End If
        Catch ex As Exception
            Debug.WriteLine("Error pixelating: " & ex.Message)
        End Try
    End Sub

    ' Ajout d'une méthode pour déboguer les coordonnées
    Private Sub DebugCoordinates()
        Try
            Dim x As Integer, y As Integer, size As Integer

            If Integer.TryParse(txtCroppedX.Text, x) AndAlso
               Integer.TryParse(txtCroppedY.Text, y) AndAlso
               Integer.TryParse(txtCroppedWidth.Text, size) Then
                Debug.WriteLine($"Coordinates: X={x}, Y={y}, Size={size}")
            End If
        Catch ex As Exception
            Debug.WriteLine("Error debugging coordinates: " & ex.Message)
        End Try
    End Sub
End Class