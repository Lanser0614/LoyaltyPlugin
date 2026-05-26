using System;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure
{
    public sealed class SystemClock
    {
        public DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }
}
