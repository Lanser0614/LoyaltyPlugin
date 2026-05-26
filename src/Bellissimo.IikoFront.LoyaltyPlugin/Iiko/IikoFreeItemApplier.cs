using System.Collections.Generic;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoFreeItemApplier
    {
        private readonly PluginLogger logger;
        public IikoFreeItemApplier(PluginLogger logger) { this.logger = logger; }

        public void ApplyFreeItems(IReadOnlyCollection<FreeItemDto> freeItems)
        {
            // TODO(iiko-sdk): 1) add product with zero price if allowed; 2) fallback to 100% selective discount via ChangeSelectiveDiscount.
            logger.Info($"Apply free items via SDK TODO. Count={freeItems?.Count ?? 0}");
        }
    }
}
