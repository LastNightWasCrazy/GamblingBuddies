using System.Reflection.Metadata;

namespace GamblingBuddies.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        public int PaymentMethodId { get; set; }
        public PaymentMethodDictionary PaymentMethod { get; set; }

        public int PaymentStatusId { get; set; }
        public PaymentStatusDictionary PaymentStatus { get; set; }

        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? ExternalOrderId { get; set; }
        public string? PaymentProviderOrderId { get; set; }
        public string? PaymentProvider { get; set; }

        public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
