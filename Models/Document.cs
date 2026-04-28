using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        public string Title { get; set; }

        public string DocumentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedByUserId { get; set; }

        public int? ReservationId { get; set; }

        public SystemUser CreatedByUser { get; set; }

        public Reservation Reservation { get; set; }

        public ICollection<DocumentFile> DocumentFile { get; set; }
    }
}
