using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class PlayerCreateDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;
    }

    public class GameCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int GameCategoryId { get; set; }

        public string? Description { get; set; }
    }

    public class HallCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int HallTypeId { get; set; }

        public string? Description { get; set; }
    }

    public class GameTableCreateDto
    {
        [Required]
        public int HallId { get; set; }

        [Range(1, 999)]
        public int TableNumber { get; set; }

        [Range(1, 20)]
        public int MinPlayers { get; set; }

        [Range(1, 20)]
        public int MaxPlayers { get; set; }
    }
}