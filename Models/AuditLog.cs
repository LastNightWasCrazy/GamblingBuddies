using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? SystemUserId { get; set; }

        public string Action { get; set; }

        public string EntityName { get; set; }

        public int? EntityId { get; set; }

        public string Details { get; set; }

        public DateTime CreatedAt { get; set; }

        public string IpAddress { get; set; }

        public SystemUser SystemUser { get; set; }
    }
}
