namespace GamblingBuddies.Models
{
    public class PaymentTransaction
    {
        public int PaymentTransactionId { get; set; }

        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        public string ExternalTransactionId { get; set; } = null!;
        public string ProviderResponseCode { get; set; } = null!;
        public string ProviderResponseMessage { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
