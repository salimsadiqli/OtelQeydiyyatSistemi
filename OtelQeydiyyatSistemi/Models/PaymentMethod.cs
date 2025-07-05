using System.ComponentModel.DataAnnotations;

namespace OtelQeydiyyatSistemi.Models
{
    public enum PaymentMethod
    {
        [Display(Name = "Nağd")]
        Cash,
        
        [Display(Name = "Kart")]
        Card
    }
}
