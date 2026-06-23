using System;
using System.Globalization;
using System.IO;

namespace BgLight
{
    public class Logger
    {
        private readonly string _logPath;

        public Logger(string logPath)
        {
            _logPath = logPath;
        }

        public void Error(string message, Exception ex = null)
        {
            try
            {
                var dir = Path.GetDirectoryName(_logPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                var line = "[" + stamp + "] ERROR: " + message;
                if (ex != null)
                {
                    line += " | " + ex.GetType().Name + ": " + ex.Message;
                }

                File.AppendAllText(_logPath, line + Environment.NewLine);
            }
            catch
            {
                // logging should never block the application
            }
        }
    }
}
