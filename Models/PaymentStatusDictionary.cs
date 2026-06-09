using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class PaymentStatusDictionary
    {
        [Key]
        public int PaymentStatusId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
