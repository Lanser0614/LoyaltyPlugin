using System;
using System.IO;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure
{
    public sealed class PluginLogger : IDisposable
    {
        private readonly string logFilePath;
        private readonly object sync = new object();

        public PluginLogger(string logDirectory)
        {
            Directory.CreateDirectory(logDirectory);
            logFilePath = Path.Combine(logDirectory, "bellissimo-loyalty-plugin.log");
        }

        public void Info(string message) => Write("INFO", message);
        public void Error(string message, Exception ex = null) => Write("ERROR", ex == null ? message : message + " :: " + ex);

        public string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 5) return "***";
            return phone.Substring(0, 4) + new string('*', Math.Max(0, phone.Length - 6)) + phone.Substring(phone.Length - 2);
        }

        private void Write(string level, string message)
        {
            lock (sync)
            {
                File.AppendAllText(logFilePath, $"{DateTime.UtcNow:O} [{level}] {message}{Environment.NewLine}");
            }
        }

        public void Dispose() { }
    }
}
