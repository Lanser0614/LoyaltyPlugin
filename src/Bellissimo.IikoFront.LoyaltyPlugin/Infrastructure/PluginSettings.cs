using System;
using System.Configuration;
using System.IO;
using System.Reflection;

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
            var map = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config"
            };
            var cfg = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            return new PluginSettings
            {
                ApiBaseUrl = Get(cfg, "ApiBaseUrl", "http://localhost:8080"),
                BasicAuthLogin = Get(cfg, "BasicAuthLogin", string.Empty),
                BasicAuthPassword = Get(cfg, "BasicAuthPassword", string.Empty),
                BranchId = ParseInt(cfg, "BranchId", 10),
                TerminalGroupId = Get(cfg, "TerminalGroupId", "iiko-terminal-group-id"),
                PosId = Get(cfg, "PosId", Environment.MachineName),
                HttpTimeout = TimeSpan.FromSeconds(ParseInt(cfg, "HttpTimeoutSeconds", 10)),
                LogDirectory = Get(cfg, "LogDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"))
            };
        }

        private static string Get(Configuration cfg, string key, string defaultValue)
        {
            return cfg.AppSettings.Settings[key]?.Value ?? defaultValue;
        }

        private static int ParseInt(Configuration cfg, string key, int defaultValue)
        {
            int value;
            return int.TryParse(cfg.AppSettings.Settings[key]?.Value, out value) ? value : defaultValue;
        }
    }
}
