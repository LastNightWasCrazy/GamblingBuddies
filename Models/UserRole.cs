namespace GamblingBuddies.Models
{
    public class UserRole
    {
        public int UserRoleId { get; set; }

        public int SystemUserId { get; set; }
        public SystemUser SystemUser { get; set; }

        public int RoleDictionaryId { get; set; }
        public RoleDictionary RoleDictionary { get; set; }
    }
}