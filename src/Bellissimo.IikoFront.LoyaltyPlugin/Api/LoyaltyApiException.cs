using System;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Api
{
    public sealed class LoyaltyApiException : Exception
    {
        public LoyaltyApiException(string code, string message) : base(message) { Code = code; }
        public string Code { get; }
    }
}
