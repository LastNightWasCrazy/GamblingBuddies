using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Models
{
    public class GameCreateViewModel
    {
        [Required(ErrorMessage = "Nazwa gry jest wymagana.")]
        [Display(Name = "Nazwa gry")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wybierz kategorię gry.")]
        [Display(Name = "Kategoria")]
        public int GameCategoryId { get; set; }

        [Display(Name = "Opis gry")]
        public string? Description { get; set; }

        [Display(Name = "Gra aktywna")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Nazwa wariantu jest wymagana.")]
        [Display(Name = "Nazwa wariantu")]
        public string VariantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opis zasad wariantu jest wymagany.")]
        [Display(Name = "Opis zasad wariantu")]
        public string RulesDescription { get; set; } = string.Empty;

        [Range(0.01, 999999, ErrorMessage = "Minimalna stawka musi być większa od 0.")]
        [Display(Name = "Minimalna stawka")]
        public decimal DefaultMinBet { get; set; } = 5;

        [Range(0.01, 999999, ErrorMessage = "Maksymalna stawka musi być większa od 0.")]
        [Display(Name = "Maksymalna stawka")]
        public decimal DefaultMaxBet { get; set; } = 200;

        [Display(Name = "Wariant aktywny")]
        public bool VariantIsActive { get; set; } = true;
    }
}
