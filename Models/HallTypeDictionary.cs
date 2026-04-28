using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class HallTypeDictionary
    {
        [Key]
        public int HallTypeId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Hall> Halls { get; set; }
    }
}
