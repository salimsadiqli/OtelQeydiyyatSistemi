using System.ComponentModel.DataAnnotations;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Düzgün email formatı deyil")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Display(Name = "Məni xatırla")]
        public bool RememberMe { get; set; }
    }
}
