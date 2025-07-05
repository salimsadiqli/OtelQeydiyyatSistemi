using System.ComponentModel.DataAnnotations;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı deyil")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ad tələb olunur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Display(Name = "Ünvan")]
        public string Address { get; set; }

        [Display(Name = "Admin")]
        public bool IsAdmin { get; set; }
    }
}
