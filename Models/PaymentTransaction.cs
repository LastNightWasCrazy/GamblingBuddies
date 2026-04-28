namespace GamblingBuddies.Models
{
    public class PaymentTransaction
    {
        public int PaymentTransactionId { get; set; }

        public int PaymentId { get; set; }
        public Payment Payment { get; set; }

        public string ExternalTransactionId { get; set; }
        public string ProviderResponseCode { get; set; }
        public string ProviderResponseMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
