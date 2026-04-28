using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class PaymentMethodDictionary
    {
        [Key]
        public int PaymentMethodId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
