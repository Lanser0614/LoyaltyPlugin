using System;
using System.Linq;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Resto.Front.Api;

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

            var session = PluginContext.Operations.CreateEditSession();
            session.AddFlexibleSumDiscount(discountType, order, (double)response.total_discount_amount);
            PluginContext.Operations.SubmitChanges(PluginContext.Operations.GetCredentials(), session);

            logger.Info($"Applied flexible sum discount {response.total_discount_amount} to order {iikoOrderId}");
        }
    }
}
