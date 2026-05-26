using System;
namespace Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos { public sealed class RewardDto { public long customer_coupon_id { get; set; } public long coupon_id { get; set; } public string name { get; set; } public string action_type { get; set; } public DateTimeOffset? expires_at { get; set; } public override string ToString() => name; } }
