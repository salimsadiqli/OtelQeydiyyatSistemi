namespace OtelQeydiyyatSistemi.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tək, Cüt, Suit və s.
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        
        // Otaqlar ilə əlaqə
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
