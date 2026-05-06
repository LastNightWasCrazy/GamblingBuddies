using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        public int ReservationStatusId { get; set; }
        public ReservationStatusDictionary ReservationStatus { get; set; }

        public DateTime ReservedAt { get; set; }

        public ICollection<ReservationSeat> ReservationSeats { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}