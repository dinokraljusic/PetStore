namespace Pet_Store.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Country { get; set; }
    }
}