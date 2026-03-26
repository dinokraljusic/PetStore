using Pet_Store.Models;
using System.Collections.Generic;

namespace Pet_Store.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Toy> Toys { get; set; } = new List<Toy>();
        public IEnumerable<ToyCategory> Categories { get; set; } = new List<ToyCategory>();
        public IEnumerable<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();

        public int? SelectedCategoryId { get; set; }
        public int? SelectedManufacturerId { get; set; }
    }
}
