using System;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class SystemUser
    {
        public int SystemUserId { get; set; }

        [Required]
        public string Login { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }


        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<WorkShift> WorkShiftsCreated { get; set; } = new List<WorkShift>();
    }
}