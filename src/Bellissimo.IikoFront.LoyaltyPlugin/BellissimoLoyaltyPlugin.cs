using System;
using Bellissimo.IikoFront.LoyaltyPlugin.Api;
using Bellissimo.IikoFront.LoyaltyPlugin.Iiko;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Bellissimo.IikoFront.LoyaltyPlugin.UI;
using Resto.Front.Api;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Data.Organization;

namespace Bellissimo.IikoFront.LoyaltyPlugin
{
    // TODO: Replace with the real module id from the iiko developer portal.
    [PluginLicenseModuleId(0)]
    public sealed class BellissimoLoyaltyPlugin : IFrontPlugin
    {
        private readonly PluginSettings settings;
        private readonly PluginLogger logger;
        private readonly LoyaltyApiClient apiClient;
        private readonly IikoOrderSnapshotBuilder snapshotBuilder;
        private readonly IikoDiscountApplier discountApplier;
        private readonly IikoFreeItemApplier freeItemApplier;
        private IDisposable loyaltyActionSubscription;

        static BellissimoLoyaltyPlugin()
        {
            // Required by iikoFront plugin contract.
        }

        public BellissimoLoyaltyPlugin()
        {
            settings = PluginSettings.Load();
            logger = new PluginLogger(settings.LogDirectory);
            apiClient = new LoyaltyApiClient(settings, logger);
            snapshotBuilder = new IikoOrderSnapshotBuilder(logger);
            discountApplier = new IikoDiscountApplier(logger);
            freeItemApplier = new IikoFreeItemApplier(logger);

            RegisterLoyaltyAction();
        }

        private void RegisterLoyaltyAction()
        {
            loyaltyActionSubscription = PluginContext.Operations.AddButtonToOrderView(
                "bellissimo-loyalty",
                "Loyalty",
                orderId => OpenLoyaltyWindow());
        }

        internal void OpenLoyaltyWindow()
        {
            var vm = new LoyaltyViewModel(
                apiClient,
                logger,
                settings,
                new IikoCredentialsProvider(settings),
                snapshotBuilder,
                discountApplier,
                freeItemApplier,
                new IdempotencyKeyFactory(),
                new SystemClock());

            StaWindowRunner.Run(() => new LoyaltyWindow(vm));
        }

        public void Dispose()
        {
            loyaltyActionSubscription?.Dispose();
            apiClient?.Dispose();
            logger?.Dispose();
        }
    }
}
