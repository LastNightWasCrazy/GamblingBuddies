using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamblingBuddies.Models
{
    public class RegistrationRequest
    {
        [Key]
        public int RegistrationRequestId { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Telefon jest wymagany")]
        [Display(Name = "Numer telefonu")]
        [Phone(ErrorMessage = "Podaj poprawny numer telefonu")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Login jest wymagany")]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [Display(Name = "Hasło")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Display(Name = "Data zgłoszenia")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Data przetworzenia")]
        public DateTime? ProcessedAt { get; set; }

        [Display(Name = "Przetworzone przez")]
        public int? ProcessedByUserId { get; set; }

        [ForeignKey("ProcessedByUserId")]
        public SystemUser ProcessedByUser { get; set; }
    }
}