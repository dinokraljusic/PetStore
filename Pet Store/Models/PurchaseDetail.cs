namespace Pet_Store.Models
{
    public class PurchaseDetail
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public Purchase Purchase { get; set; } = null!;

        public Toy Toy { get; set; } = null!;
    }
}