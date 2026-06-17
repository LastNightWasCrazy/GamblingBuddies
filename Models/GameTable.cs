namespace GamblingBuddies.Models
{
    public class GameTable
    {
        public int GameTableId { get; set; }

        public int HallId { get; set; }
        public Hall Hall { get; set; }

        [Range(1, 999)]
        public int TableNumber { get; set; }

        [Range(1, 20)]
        public int MinPlayers { get; set; }

        [Range(1, 20)]
        public int MaxPlayers { get; set; }
        public bool IsActive { get; set; }

        public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
        public ICollection<GameTableGame> GameTableGames { get; set; } = new List<GameTableGame>();
    }
}