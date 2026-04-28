using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        // FK -> Player
        public int PlayerId { get; set; }
        public Player Player { get; set; }

        // FK -> GameSession
        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        // FK -> ReservationStatusDictionary
        public int ReservationStatusId { get; set; }
        public ReservationStatusDictionary ReservationStatus { get; set; }

        public DateTime ReservedAt { get; set; }

        public ICollection<ReservationSeat> ReservationSeats { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}