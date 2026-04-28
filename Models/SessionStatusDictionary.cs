using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class SessionStatusDictionary
    {
        [Key]
        public int SessionStatusId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<GameSession> GameSessions { get; set; }
    }
}
