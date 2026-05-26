using System.Collections.Generic;
using System.Linq;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Resto.Front.Api;

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

            var menuProducts = PluginContext.Operations.GetProductTypes().ToDictionary(p => p.Id.ToString(), p => p);
            var session = PluginContext.Operations.CreateEditSession();

            foreach (var freeItem in freeItems)
            {
                if (!menuProducts.TryGetValue(freeItem.iiko_product_id, out var product))
                {
                    logger.Info($"Skip free item: product not found {freeItem.iiko_product_id}");
                    continue;
                }

                session.AddOrderProductItem(
                    freeItem.quantity,
                    product,
                    order,
                    PluginContext.Operations.GetCredentials());
            }

            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), session);
            logger.Info($"Applied free items. Count={freeItems.Count}");
        }
    }
}
