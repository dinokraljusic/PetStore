using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pet_Store.ViewModels
{
    public class PurchaseItemViewModel
    {
        public int ToyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class PurchaseCreateViewModel
    {
        public List<PurchaseItemViewModel> Items { get; set; } = new();

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string CreditCardNumber { get; set; } = string.Empty;

        public string? Address { get; set; }
    }
}
