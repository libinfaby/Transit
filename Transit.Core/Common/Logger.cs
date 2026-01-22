using System;
using System.IO;

namespace Transit.Core.Common
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logPath = "app.log";

        public static void SetLogPath(string path)
        {
            _logPath = path;
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
                }
            }
            catch (Exception)
            {
                // Inspecting log failure is risky, better to ignore or debug print
                System.Diagnostics.Debug.WriteLine($"Failed to log: {message}");
            }
        }
        
        public static void LogError(string context, Exception ex)
        {
             Log($"ERROR [{context}]: {ex.Message} \n {ex.StackTrace}");
        }
    }
}
