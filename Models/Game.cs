namespace GamblingBuddies.Models
{
    public class Game
    {
        public int GameId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public int GameCategoryId { get; set; }
        public GameCategoryDictionary GameCategory { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
        public ICollection<GameVariant> GameVariants { get; set; } = new List<GameVariant>();

        public ICollection<GameTableGame> GameTableGames { get; set; } = new List<GameTableGame>();
    }
}