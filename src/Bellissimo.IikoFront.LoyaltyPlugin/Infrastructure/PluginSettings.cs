using System;
using System.Configuration;
using System.IO;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure
{
    public sealed class PluginSettings
    {
        public string ApiBaseUrl { get; private set; }
        public string BasicAuthLogin { get; private set; }
        public string BasicAuthPassword { get; private set; }
        public int BranchId { get; private set; }
        public string TerminalGroupId { get; private set; }
        public string PosId { get; private set; }
        public TimeSpan HttpTimeout { get; private set; }
        public string LogDirectory { get; private set; }

        public static PluginSettings Load()
        {
            return new PluginSettings
            {
                ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:8080",
                BasicAuthLogin = ConfigurationManager.AppSettings["BasicAuthLogin"] ?? string.Empty,
                BasicAuthPassword = ConfigurationManager.AppSettings["BasicAuthPassword"] ?? string.Empty,
                BranchId = ParseInt("BranchId", 10),
                TerminalGroupId = ConfigurationManager.AppSettings["TerminalGroupId"] ?? "iiko-terminal-group-id",
                PosId = ConfigurationManager.AppSettings["PosId"] ?? Environment.MachineName,
                HttpTimeout = TimeSpan.FromSeconds(ParseInt("HttpTimeoutSeconds", 10)),
                LogDirectory = ConfigurationManager.AppSettings["LogDirectory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")
            };
        }

        private static int ParseInt(string key, int defaultValue)
        {
            int value;
            return int.TryParse(ConfigurationManager.AppSettings[key], out value) ? value : defaultValue;
        }
    }
}
