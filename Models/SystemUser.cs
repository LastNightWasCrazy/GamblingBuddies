using System;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class SystemUser
    {
        public int SystemUserId { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }


        public ICollection<UserRole> UserRoles { get; set; }
    }
}