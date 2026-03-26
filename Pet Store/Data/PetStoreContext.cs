using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pet_Store.Models;

namespace Pet_Store.Data
{
    public class PetStoreContext : IdentityDbContext<AppUser>
    {
        public PetStoreContext(DbContextOptions<PetStoreContext> options) : base(options) { }

        public DbSet<Toy> Toys { get; set; }
        public DbSet<Purchase> Purchases { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Manufacturer> Manufacturers { get; set; }

        public DbSet<PurchaseDetail> PurchaseDetails { get; set; }

        public DbSet<ToyCategory> ToyCategories { get; set; }
    }
}
