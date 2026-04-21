using System;

namespace GamblingBuddies.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}