using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoDiscountApplier
    {
        private readonly PluginLogger logger;
        public IikoDiscountApplier(PluginLogger logger) { this.logger = logger; }

        public void ApplyDiscounts(ApplyResponse response)
        {
            // TODO(iiko-sdk): AddFlexibleSumDiscount exact signature and SubmitChanges edit session.
            var discountType = ResolveLoyaltyDiscountType();
            logger.Info($"Apply discounts via SDK TODO. DiscountType={discountType}, amount={response.total_discount_amount}");
        }

        private string ResolveLoyaltyDiscountType()
        {
            // TODO(iiko-sdk): Resolve discount type by external id/name.
            return "TODO_LOYALTY_DISCOUNT_TYPE";
        }
    }
}
