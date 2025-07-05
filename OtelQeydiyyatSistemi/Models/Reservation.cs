using OtelQeydiyyatSistemi.ViewModels;

namespace OtelQeydiyyatSistemi.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        CheckedIn,
        CheckedOut,
        Cancelled
    }

    public class Reservation
    {
        public int Id { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public decimal TotalPrice { get; set; }
        public string SpecialRequests { get; set; }
        public int Adults { get; set; } = 1;
        public int Children { get; set; } = 0;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        
        // İstifadəçi ilə əlaqə
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        
        // Otaq ilə əlaqə
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }
        
        // Qalma müddətini hesablama
        public int GetDurationInDays()
        {
            return (CheckOutDate - CheckInDate).Days;
        }
    }
}
