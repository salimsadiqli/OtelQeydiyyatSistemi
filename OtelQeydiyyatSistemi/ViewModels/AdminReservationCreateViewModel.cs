using System;
using System.ComponentModel.DataAnnotations;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class AdminReservationCreateViewModel : ReservationCreateViewModel
    {
        [Required(ErrorMessage = "Müştəri seçilməlidir")]
        [Display(Name = "Müştəri")]
        public string UserId { get; set; }
    }
}
