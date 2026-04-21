namespace GamblingBuddies.Models
{
    public class UserRole
    {
        public int UserRoleId { get; set; }

        public int SystemUserId { get; set; }
        public int RoleId { get; set; }

        public SystemUser SystemUser { get; set; }
        public Role Role { get; set; }
    }
}