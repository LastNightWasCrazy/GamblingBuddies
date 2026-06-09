namespace GamblingBuddies.Models
{
    public class GameTableGame
    {
        public int GameTableId { get; set; }
        public GameTable GameTable { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
