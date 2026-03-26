namespace Pet_Store.Models
{
    public class Toy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public ToyCategory? Category {  get; set; }
        public int? ManufacturerId { get; set; }

        public int QuantityAvailable { get; set; } = 0;
        public Manufacturer? Manufacturer { get; set; }
    }
}
