Imports System
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Text.RegularExpressions


Public Class Form1

    Dim firstRun As Integer = 0
    Dim lastLog As String = String.Empty
    Dim day As String() = New String() {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"}
    Dim selFolder As String

    Private Function folderSelect()
        Dim fbd As New FolderBrowserDialog
        fbd.Description = "ガンダムオンラインのログファイル保存フォルダを選択してください"
        fbd.SelectedPath = "C:\Program Files (x86)\Gameone\Gundam_Online\GundamOnline\chat"
        If fbd.ShowDialog(Me) = DialogResult.OK Then
            TextBox1.Text = fbd.SelectedPath + "\" + DateTime.Now.ToString("yyyy_MM_dd(") + day(CInt(DateTime.Now.DayOfWeek)) + ").log"
            selFolder = fbd.SelectedPath
            TextBox1.Visible = False
            TextBox2.Visible = True
        End If
        Return System.IO.Path.GetDirectoryName(TextBox1.Text)
    End Function

    Private Sub iniCheck()
        Dim fileName As String = "Config.ini"
        Dim fileEncode As String = "shift_jis"
        If System.IO.File.Exists(fileName) Then
            'ファイルは存在します
            Try
                Dim sr As New System.IO.StreamReader(fileName, System.Text.Encoding.GetEncoding(fileEncode))
                Dim s As String = sr.ReadToEnd()
                sr.Close()
                TextBox1.Text = s + "\" + DateTime.Now.ToString("yyyy_MM_dd(") + day(CInt(DateTime.Now.DayOfWeek)) + ").log"
            Catch ex As Exception

            End Try
        Else
            'ファイルは存在しません
            Dim hStream As System.IO.FileStream
            Try
                Try
                    hStream = System.IO.File.Create(fileName)
                Finally
                    If Not hStream Is Nothing Then
                        hStream.Close()
                    End If
                End Try
            Finally
                If Not hStream Is Nothing Then
                    Dim cDisposable As System.IDisposable = hStream
                    cDisposable.Dispose()
                End If
            End Try
            Try
                Dim sw As New System.IO.StreamWriter(fileName, False, System.Text.Encoding.GetEncoding(fileEncode))
                sw.Write(folderSelect())
                sw.Close()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        iniCheck()
        Timer1.Start()
    End Sub

    Private Sub TextBox1_TextChanged_1(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Dim fileName As String = TextBox1.Text
        If File.Exists(fileName) Then
            Button1.Text = "■"
            Button1.Enabled = True
            Timer1.Enabled = True
            Me.TopMost = True
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        '指定されたファイルをsに読み込む

        'フォルダの決定
        Dim targetFolder As String = Path.GetFileName(TextBox1.Text)
        targetFolder = TextBox1.Text.Replace(targetFolder, String.Empty)
        'ファイル名の決定
        Dim fileName As String
        fileName = "\" + DateTime.Now.ToString("yyyy_MM_dd(") + day(CInt(DateTime.Now.DayOfWeek)) + ").log"

        Using fs As New FileStream(targetFolder + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Dim sr As TextReader = New StreamReader(fs, System.Text.Encoding.GetEncoding("UTF-16"))
            Dim s As String = sr.ReadToEnd()
            sr.Close()

            '差分を取得しsに格納
            If firstRun = 0 Then
                firstRun = 1
                lastLog = s
            Else
                If lastLog = s Then
                    s = String.Empty
                Else
                    Dim s2 As String = s
                    s = s.Replace(lastLog, String.Empty) '読み込んだログから前回のログを消すことで差分を取得する
                    lastLog = s2

                    While s.Length >= 10
                        '未翻訳文の最初の行のみを取得
                        Dim r As New Regex("^([^\r\n]+)", RegexOptions.IgnoreCase)
                        Dim m As Match = r.Match(s)
                        While m.Success
                            TextBox3.Text = Regex.Replace(m.Value, "%0d%0a%5b", String.Empty)
                            m = m.NextMatch()
                        End While

                        'すでに取得した文字列は取り除く
                        s = s.Substring(s.IndexOf(vbCrLf) + vbCrLf.Length)
                        TextBox4.Text = s

                        '差分をGoogleに投げる
                        Try
                            Dim time As String = TextBox3.Text.Substring(1, 8)
                            TextBox3.Text = TextBox3.Text.Substring(11)
                            Dim name As String = TextBox3.Text.Substring(0, TextBox3.Text.IndexOf("："))
                            Dim text As String = TextBox3.Text.Substring(TextBox3.Text.IndexOf("：") + 1)

                            Dim url As String = "https://translate.google.com/m?hl=ja&sl=zh-CN&tl=ja&ie=UTF-8&prev=_m&q=" + System.Web.HttpUtility.UrlEncode(text)
                            Dim wc As New System.Net.WebClient()
                            Dim source As String = wc.DownloadString(url)
                            wc.Dispose()

                            '受け取った翻訳結果をtextbox3.textへ
                            r = New Regex("<div dir\=""ltr"" class\=""t0"">([^<]+)</div>", RegexOptions.IgnoreCase)
                            m = r.Match(source)
                            While m.Success
                                Dim patternStr As String = "<.*?>"
                                TextBox3.Text = Regex.Replace(m.Value, patternStr, String.Empty)
                                m = m.NextMatch()
                            End While

                            TextBox2.Text = TextBox2.Text & Environment.NewLine & "[" & time & "] " & name & "：" & TextBox3.Text
                            TextBox2.SelectionStart = TextBox2.Text.Length
                            TextBox2.Focus()
                            TextBox2.ScrollToCaret()

                            If CheckBox1.Checked = True Then Clipboard.SetText(source)
                        Catch ex As Exception
                        End Try
                    End While
                End If
            End If



        End Using

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Button1.Text = "■" Then
            Button1.Text = "▶"
            Timer1.Stop()
            Me.TopMost = False
        Else
            Button1.Text = "■"
            Timer1.Start()
            Me.TopMost = True
        End If
    End Sub

    Private Sub Form1_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        If Me.Size.Width < 389 Then Me.Width = 389
        If Me.Size.Height < 220 Then Me.Height = 220
        TextBox2.Width = Me.Size.Width - 12
        TextBox2.Height = Me.Size.Height - 61
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Me.Opacity = 0.5 Then
            Me.Opacity = 1
        ElseIf Me.Opacity = 1 Then
            Me.Opacity = 0.7
        ElseIf Me.Opacity = 0.7 Then
            Me.Opacity = 0.5
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If Button3.Text = "枠なし" Then
            Button3.Text = "枠あり"
            Me.FormBorderStyle = FormBorderStyle.None
            TextBox2.Width = Me.Size.Width + 4
            TextBox2.Height = Me.Size.Height - 20
        Else
            Button3.Text = "枠なし"
            Me.FormBorderStyle = FormBorderStyle.Sizable
            TextBox2.Width = Me.Size.Width - 12
            TextBox2.Height = Me.Size.Height - 61
        End If

    End Sub
End Class