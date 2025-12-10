Module Program
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim monitor As New NetSpeedMonitor
        Application.Run() ' Hält die App am Laufen
    End Sub
End Module
