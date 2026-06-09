using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class GameEditViewModel
    {
        public int GameId { get; set; }

        [Required(ErrorMessage = "Nazwa gry jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa gry może mieć maksymalnie 100 znaków.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wybierz kategorię gry.")]
        [Display(Name = "Kategoria")]
        public int GameCategoryId { get; set; }

        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Gra aktywna")]
        public bool IsActive { get; set; } = true;
    }
}
