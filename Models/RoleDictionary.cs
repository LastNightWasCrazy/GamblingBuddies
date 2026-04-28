namespace GamblingBuddies.Models
{
    public class RoleDictionary
    {
        public int RoleDictionaryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}