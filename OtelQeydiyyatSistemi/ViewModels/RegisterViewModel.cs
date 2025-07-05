using System.ComponentModel.DataAnnotations;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad tələb olunur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı deyil")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Ünvan")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [StringLength(100, ErrorMessage = "Şifrə ən azı {2} simvol uzunluğunda olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifrəni təsdiqləyin")]
        [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir")]
        public string ConfirmPassword { get; set; }
    }
}
