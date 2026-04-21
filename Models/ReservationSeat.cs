using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamblingBuddies.Models
{
    public class ReservationSeat
    {
        [Key]
        public int ReservationSeatId { get; set; }

        // FK -> Reservation
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        // FK -> Seat
        public int SeatId { get; set; }
        public Seat Seat { get; set; }

        // (opcjonalnie – jeśli chcesz kontrolować miejsca w sesji)
        public int SessionId { get; set; }
        public Session Session { get; set; }
    }
}