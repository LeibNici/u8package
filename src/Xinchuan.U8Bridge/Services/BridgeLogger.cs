using System;
using System.IO;

namespace Xinchuan.U8Bridge.Services
{
    public static class BridgeLogger
    {
        private static readonly object SyncRoot = new object();

        public static string LogDirectory
        {
            get
            {
                string configured = Environment.GetEnvironmentVariable("U8_BRIDGE_LOG_DIR");
                if (!string.IsNullOrWhiteSpace(configured))
                {
                    return configured;
                }

                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            }
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Warn(string message)
        {
            Write("WARN", message, null);
        }

        public static void Error(string message, Exception exception)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(string level, string message, Exception exception)
        {
            string line = FormatLine(level, message, exception);
            Console.WriteLine(line);

            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(LogDirectory);
                    File.AppendAllText(GetLogFilePath(), line + Environment.NewLine);
                }
            }
            catch (Exception logException)
            {
                Console.Error.WriteLine("Bridge log write failed: " + logException.Message);
            }
        }

        private static string FormatLine(string level, string message, Exception exception)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                + " [" + level + "] "
                + message;
            return exception == null
                ? line
                : line + Environment.NewLine + exception;
        }

        private static string GetLogFilePath()
        {
            string fileName = "u8-bridge-" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            return Path.Combine(LogDirectory, fileName);
        }
    }
}
