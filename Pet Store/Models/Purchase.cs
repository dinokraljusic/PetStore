using System.ComponentModel.DataAnnotations;

namespace Pet_Store.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        [Required]
        public required string CreditCardNumber { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        [Required]
        public required string Address { get; set; }
        public User User { get; set; } = null!;
        public ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    }
}
