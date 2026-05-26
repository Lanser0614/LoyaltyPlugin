using System.Collections.Generic;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoOrderSnapshotBuilder
    {
        private readonly PluginLogger logger;
        public IikoOrderSnapshotBuilder(PluginLogger logger) { this.logger = logger; }

        public OrderSnapshot BuildCurrentOrderSnapshot()
        {
            // TODO(iiko-sdk): replace stub with PluginContext.Operations.GetCurrentOrder() flow.
            logger.Info("Building order snapshot (stub for SDK-specific implementation)");
            return new OrderSnapshot { IikoOrderId = "iiko-order-123", CashierId = "cashier-123", Items = new List<OrderItemSnapshotDto>() };
        }
    }

    public sealed class OrderSnapshot
    {
        public string IikoOrderId { get; set; }
        public string CashierId { get; set; }
        public bool IsClosed { get; set; }
        public List<OrderItemSnapshotDto> Items { get; set; }
    }
}
