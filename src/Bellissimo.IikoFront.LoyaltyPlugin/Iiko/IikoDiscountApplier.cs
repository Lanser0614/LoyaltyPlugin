using System;
using System.Linq;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Resto.Front.Api;
using Resto.Front.Api.Extensions;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoDiscountApplier
    {
        private readonly PluginLogger logger;
        public IikoDiscountApplier(PluginLogger logger) { this.logger = logger; }

        public void ApplyDiscounts(ApplyResponse response, string iikoOrderId)
        {
            if (response == null || response.total_discount_amount <= 0)
                return;

            var discountType = PluginContext.Operations.GetDiscountTypes()
                .First(d => !d.Deleted && d.IsActive && d.DiscountByFlexibleSum && d.Name == "BellissimoLoyalty");

            var order = PluginContext.Operations.GetOrders()
                .First(o => o.Id.ToString() == iikoOrderId);

            var credentials = PluginContext.Operations.GetDefaultCredentials();
            PluginContext.Operations.AddFlexibleSumDiscount(response.total_discount_amount, discountType, order, credentials);

            logger.Info($"Applied flexible sum discount {response.total_discount_amount} to order {iikoOrderId}");
        }
    }
}
