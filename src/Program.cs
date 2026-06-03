namespace MihomoDashboard;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportCrash(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                ReportCrash(exception);
            }
        };

        try
        {
            ApplicationConfiguration.Initialize();

            var startMinimized = args.Any(arg => string.Equals(arg, "--minimized", StringComparison.OrdinalIgnoreCase));
            var startCore = args.Any(arg => string.Equals(arg, "--start-core", StringComparison.OrdinalIgnoreCase));
            using var form = new MainForm(startMinimized, startCore);
            Application.Run(form);
        }
        catch (Exception exception)
        {
            ReportCrash(exception);
        }
    }

    private static void ReportCrash(Exception exception)
    {
        var logPath = Path.Combine(AppSettings.SettingsDirectory, "crash.log");
        Directory.CreateDirectory(AppSettings.SettingsDirectory);
        File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception}{Environment.NewLine}{Environment.NewLine}");

        try
        {
            MessageBox.Show(
                $"Mihomo Dashboard failed to start.\n\n{exception.Message}\n\nCrash log:\n{logPath}",
                "Mihomo Dashboard",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
            // The log file is the fallback when UI cannot be displayed.
        }
    }
}
