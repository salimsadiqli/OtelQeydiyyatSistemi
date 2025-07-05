using Microsoft.AspNetCore.Identity;

namespace OtelQeydiyyatSistemi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        // Rezervasiyalar ilə əlaqə
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
