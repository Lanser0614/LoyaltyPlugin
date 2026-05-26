namespace Bellissimo.IikoFront.LoyaltyPlugin.Infrastructure
{
    public sealed class IdempotencyKeyFactory
    {
        public string ForApply(string iikoOrderId, string previewId) => $"{iikoOrderId}:reward-apply:{previewId}";
        public string ForCancel(string iikoOrderId, long applicationId) => $"{iikoOrderId}:reward-cancel:{applicationId}";
    }
}
