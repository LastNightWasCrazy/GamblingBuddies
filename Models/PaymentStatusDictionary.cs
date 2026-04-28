namespace GamblingBuddies.Models
{
    public class PaymentStatusDictionary
    {
        [Key]
        public int PaymentStatusId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
