namespace GamblingBuddies.Models
{
    public class GameTable
    {
        public int GameTableId { get; set; }

        public int HallId { get; set; }
        public Hall Hall { get; set; }
        public int TableNumber { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    }
}