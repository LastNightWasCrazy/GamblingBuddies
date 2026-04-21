using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class ReservationStatusDictionary
    {
        [Key]
        public int ReservationStatusId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        // relacja 1:N do Reservation
        public ICollection<Reservation> Reservations { get; set; }
    }
}