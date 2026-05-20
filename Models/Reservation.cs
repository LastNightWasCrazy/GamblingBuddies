using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Gracz jest wymagany")]
        public int PlayerId { get; set; }
        public Player Player { get; set; }

        [Required(ErrorMessage = "Sesja gry jest wymagana")]
        public int GameSessionId { get; set; }
        public GameSession GameSession { get; set; }

        [Required(ErrorMessage = "Status jest wymagany")]
        public int ReservationStatusId { get; set; }
        public ReservationStatusDictionary ReservationStatus { get; set; }

        [Required(ErrorMessage = "Data rezerwacji jest wymagana")]
        public DateTime ReservedAt { get; set; }

        public ICollection<ReservationSeat> ReservationSeats { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}