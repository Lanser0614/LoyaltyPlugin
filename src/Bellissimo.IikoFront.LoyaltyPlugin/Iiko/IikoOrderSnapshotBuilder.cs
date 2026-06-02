using System.Collections.Generic;
using System.Linq;
using Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos;
using Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure;
using Resto.Front.Api;
using Resto.Front.Api.Data.Orders;

namespace Bellissimo.IikoFront.LoyaltyPlugin.Iiko
{
    public sealed class IikoOrderSnapshotBuilder
    {
        private readonly PluginLogger logger;
        public IikoOrderSnapshotBuilder(PluginLogger logger) { this.logger = logger; }

        public OrderSnapshot BuildCurrentOrderSnapshot()
        {
            var order = PluginContext.Operations
                .GetOrders()
                .FirstOrDefault(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Bill);

            var credentials = PluginContext.Operations.GetDefaultCredentials();
            var cashier = PluginContext.Operations.GetUser(credentials)?.Id.ToString() ?? "unknown";

            return new OrderSnapshot
            {
                IikoOrderId = order?.Id.ToString() ?? string.Empty,
                CashierId = cashier,
                IsClosed = order == null,
                Items = order?.Items
                    .OfType<IOrderProductItem>()
                    .Select(i => new OrderItemSnapshotDto
                    {
                        line_id = i.Id.ToString(),
                        type = "product",
                        iiko_product_id = i.Product.Id.ToString(),
                        iiko_group_id = i.Product.Category?.Id.ToString() ?? string.Empty,
                        quantity = i.Amount,
                        total_price = (long)(i.Price * i.Amount)
                    })
                    .ToList() ?? new List<OrderItemSnapshotDto>()
            };
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
