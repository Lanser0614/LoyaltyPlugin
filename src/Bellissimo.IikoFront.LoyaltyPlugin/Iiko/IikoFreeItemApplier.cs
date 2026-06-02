using System;
using System.Collections.Generic;
using System.Linq;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Resto.Front.Api;
using Resto.Front.Api.Extensions;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoFreeItemApplier
    {
        private readonly PluginLogger logger;
        public IikoFreeItemApplier(PluginLogger logger) { this.logger = logger; }

        public void ApplyFreeItems(IReadOnlyCollection<FreeItemDto> freeItems)
        {
            if (freeItems == null || freeItems.Count == 0)
                return;

            var order = PluginContext.Operations.GetOrders().FirstOrDefault();
            if (order == null)
            {
                logger.Info("Skip free items applying: no active order");
                return;
            }

            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var guest = order.Guests.FirstOrDefault();

            foreach (var freeItem in freeItems)
            {
                if (!Guid.TryParse(freeItem.iiko_product_id, out var productId))
                {
                    logger.Info($"Skip free item: invalid product id {freeItem.iiko_product_id}");
                    continue;
                }

                var product = PluginContext.Operations.TryGetProductById(productId);
                if (product == null)
                {
                    logger.Info($"Skip free item: product not found {freeItem.iiko_product_id}");
                    continue;
                }

                PluginContext.Operations.AddOrderProductItem(
                    freeItem.quantity,
                    product,
                    order,
                    guest,
                    null,
                    credentials);
            }

            logger.Info($"Applied free items. Count={freeItems.Count}");
        }
    }
}
