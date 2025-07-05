using System;
using System.ComponentModel.DataAnnotations;
using OtelQeydiyyatSistemi.Models;

namespace OtelQeydiyyatSistemi.ViewModels
{
    public class ReservationViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Giriş tarixi")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }
        
        [Display(Name = "Çıxış tarixi")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }
        
        [Display(Name = "Rezervasiya tarixi")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }
        
        [Display(Name = "Status")]
        public ReservationStatus Status { get; set; }
        
        [Display(Name = "Ümumi qiymət")]
        public decimal TotalPrice { get; set; }
        
        [Display(Name = "Xüsusi istəklər")]
        public string SpecialRequests { get; set; }
        
        [Display(Name = "Böyüklərin sayı")]
        public int Adults { get; set; }
        
        [Display(Name = "Uşaqların sayı")]
        public int Children { get; set; }
        
        [Display(Name = "Müştəri")]
        public string UserFullName { get; set; }
        
        [Display(Name = "Otaq")]
        public string RoomInfo { get; set; }
        
        public int RoomId { get; set; }
        public string UserId { get; set; }
    }
}
