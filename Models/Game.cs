namespace GamblingBuddies.Models
{
    public class Game
    {
        public int GameId { get; set; }

        public string Name { get; set; }

        public int GameCategoryId { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }
}