using System;
using System.ComponentModel.DataAnnotations;
using OtelQeydiyyatSistemi.Models;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class ReservationCreateViewModel
    {
        [Required(ErrorMessage = "Giriş tarixi tələb olunur")]
        [Display(Name = "Giriş tarixi")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Çıxış tarixi tələb olunur")]
        [Display(Name = "Çıxış tarixi")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Otaq seçilməlidir")]
        [Display(Name = "Otaq")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Böyüklərin sayı tələb olunur")]
        [Display(Name = "Böyüklərin sayı")]
        [Range(1, 10, ErrorMessage = "Böyüklərin sayı 1-10 arasında olmalıdır")]
        public int Adults { get; set; } = 1;

        [Display(Name = "Uşaqların sayı")]
        [Range(0, 10, ErrorMessage = "Uşaqların sayı 0-10 arasında olmalıdır")]
        public int Children { get; set; } = 0;

        [Display(Name = "Xüsusi istəklər")]
        [StringLength(500, ErrorMessage = "Xüsusi istəklər 500 simvoldan çox ola bilməz")]
        public string SpecialRequests { get; set; }
        
        [Required(ErrorMessage = "Ödəniş metodu seçilməlidir")]
        [Display(Name = "Ödəniş metodu")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}
