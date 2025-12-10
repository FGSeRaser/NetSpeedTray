Imports System.Diagnostics
Imports System.Windows.Forms

Public Class NetSpeedMonitor
    Private WithEvents notifyIcon As New NotifyIcon
    Private WithEvents timer As New Timer
    Private downloadCounter As New List(Of PerformanceCounter)
    Private uploadCounter As New List(Of PerformanceCounter)

    ' <<< NEU: Einheitsschalter >>>
    Private showInMbit As Boolean = True   ' True = Mbit/s, False = kbit/s

    Public Sub New()
        InitializeComponent()
        StartMonitoring()
    End Sub

    Private Sub InitializeComponent()
        ' Dein Icon
        notifyIcon.Icon = My.Resources.rHqh6MC  ' ggf. Namen anpassen
        notifyIcon.Visible = True
        notifyIcon.Text = "NetSpeed Monitor"

        ' <<< NEU: Kontextmenü mit kbit/s / Mbit/s >>>
        Dim menu As New ContextMenuStrip
        menu.Items.Add("kbit/s", Nothing, AddressOf MenuKbit_Click)
        menu.Items.Add("Mbit/s", Nothing, AddressOf MenuMbit_Click)
        menu.Items.Add(New ToolStripSeparator())
        menu.Items.Add("Beenden", Nothing, AddressOf MenuExit_Click)
        notifyIcon.ContextMenuStrip = menu

        timer.Interval = 1500   ' 1,5s
    End Sub

    Private Sub StartMonitoring()
        InitializeCounters()
        timer.Start()
    End Sub

    Private Sub InitializeCounters()
        Try
            Dim category As New PerformanceCounterCategory("Network Interface")
            Dim instances() As String = category.GetInstanceNames()

            For Each instance In instances
                If instance <> "_Total" AndAlso Not instance.Contains("Loopback") Then
                    downloadCounter.Add(New PerformanceCounter("Network Interface", "Bytes Received/sec", instance))
                    uploadCounter.Add(New PerformanceCounter("Network Interface", "Bytes Sent/sec", instance))
                End If
            Next
        Catch
            ' ignorieren
        End Try
    End Sub

    ' <<< NEU: Menü-Handler >>>
    Private Sub MenuKbit_Click(sender As Object, e As EventArgs)
        showInMbit = False
    End Sub

    Private Sub MenuMbit_Click(sender As Object, e As EventArgs)
        showInMbit = True
    End Sub

    Private Sub MenuExit_Click(sender As Object, e As EventArgs)
        Cleanup()
        Application.Exit()
    End Sub

    ' Timer-Tick: Anzeige aktualisieren (inkl. kbit/Mbit)
    Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
        Dim downBps As Double = 0, upBps As Double = 0

        For Each c In downloadCounter
            Try : downBps += c.NextValue() : Catch : End Try
        Next
        For Each c In uploadCounter
            Try : upBps += c.NextValue() : Catch : End Try
        Next

        Dim text As String

        If showInMbit Then
            Dim downM = downBps * 8 / 1000000.0   ' Byte/s → bit/s → Mbit/s
            Dim upM = upBps * 8 / 1000000.0
            text = $"↓{downM:F1} ↑{upM:F1} Mbit/s"
        Else
            Dim downK = downBps * 8 / 1000.0      ' Byte/s → bit/s → kbit/s
            Dim upK = upBps * 8 / 1000.0
            text = $"↓{downK:F0} ↑{upK:F0} kbit/s"
        End If

        If text.Length > 63 Then text = text.Substring(0, 60) & ".."
        notifyIcon.Text = text
    End Sub

    Private Sub Cleanup()
        timer.Stop()
        notifyIcon.Visible = False
        For Each c In downloadCounter
            Try : c.Dispose() : Catch : End Try
        Next
        For Each c In uploadCounter
            Try : c.Dispose() : Catch : End Try
        Next
    End Sub
End Class
