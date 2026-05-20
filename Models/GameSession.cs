using System;

namespace GamblingBuddies.Models
{
    public class GameSession
    {
        public int GameSessionId { get; set; }

        public int GameVariantId { get; set; }
        public int GameTableId { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        public int SessionStatusId { get; set; }

        public int CreatedByUserId { get; set; }

        public GameVariant GameVariant { get; set; }
        public GameTable GameTable { get; set; }

        public SessionStatusDictionary SessionStatus { get; set; }

        public SystemUser CreatedByUser { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}