namespace GamblingBuddies.Models
{
    public class GameTable
    {
        public int GameTableId { get; set; }

        public int HallId { get; set; }

        public int TableNumber { get; set; }

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public bool IsActive { get; set; }

        public Hall Hall { get; set; }
    }
}