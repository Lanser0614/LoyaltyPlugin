using System.Collections.Generic;
namespace Bellissimo.IikoFront.LoyaltyPlugin.Api.Dtos { public sealed class LookupResponse { public long customer_id { get; set; } public string name { get; set; } public string phone { get; set; } public BellCoinDto bellcoin { get; set; } public List<RewardDto> available_coupons { get; set; } } }
