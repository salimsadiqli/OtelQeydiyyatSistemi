namespace OtelQeydiyyatSistemi.Models
{
    public enum RoomStatus
    {
        Available,
        Occupied,
        Maintenance,
        Reserved
    }

    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public int Floor { get; set; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public string ImageUrl { get; set; }
        
        // Otaq növü ilə əlaqə
        public int RoomTypeId { get; set; }
        public virtual RoomType RoomType { get; set; }
        
        // Rezervasiyalar ilə əlaqə
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
