using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class GameCategoryDictionary
    {
        [Key]
        public int GameCategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Game> Games { get; set; }
    }
}
