using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        // FK -> Table
        public int TableId { get; set; }
        public GameTable Table { get; set; }

        public int SeatNumber { get; set; }

        // relacja do ReservationSeat
        public ICollection<ReservationSeat> ReservationSeats { get; set; }
    }
}