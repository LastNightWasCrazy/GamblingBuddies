using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class PaymentMethodDictionary
    {
        [Key]
        public int PaymentMethodId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
