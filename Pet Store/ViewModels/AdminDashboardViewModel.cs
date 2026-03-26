using Pet_Store.Models;
using System.Collections.Generic;

namespace Pet_Store.ViewModels
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<Purchase> Purchases { get; set; } = new List<Purchase>();
        public IEnumerable<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();
        public IEnumerable<ToyCategory> Categories { get; set; } = new List<ToyCategory>();
        public IEnumerable<Toy> Toys { get; set; } = new List<Toy>();
    }
}
