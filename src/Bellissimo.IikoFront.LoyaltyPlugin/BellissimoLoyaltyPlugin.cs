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
    // TODO: Replace with real GUID from iiko developer portal after obtaining developer license.
    [PluginLicenseModuleId("00000000-0000-0000-0000-000000000000")]
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
            // TODO(iiko-sdk): verify exact API for adding button/menu action in your SDK version.
            // Keep this isolated so unknown SDK signatures do not leak into business classes.
            loyaltyActionSubscription = PluginContext.Notifications.SubscribeToAnyNotification(_ => { });
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
