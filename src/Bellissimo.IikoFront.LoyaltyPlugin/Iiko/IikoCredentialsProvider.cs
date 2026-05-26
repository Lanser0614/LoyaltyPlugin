using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoCredentialsProvider
    {
        private readonly PluginSettings settings;
        public IikoCredentialsProvider(PluginSettings settings) { this.settings = settings; }
        public string TerminalGroupId => settings.TerminalGroupId;
        public string PosId => settings.PosId;
    }
}
