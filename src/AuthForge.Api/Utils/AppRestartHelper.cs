using System.Diagnostics;

namespace AuthForge.Api.Utils
{
    public static class AppRestartHelper
    {
        public static void Restart()
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;

            if (!string.IsNullOrEmpty(exePath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }

            Environment.Exit(0);
        }
    }
}