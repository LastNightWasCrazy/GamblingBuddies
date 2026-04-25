using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class ReservationSeat
    {
        [Key]
        public int ReservationSeatId { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }
    }
}