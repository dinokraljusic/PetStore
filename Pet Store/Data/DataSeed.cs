using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pet_Store.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

namespace Pet_Store.Data
{
    public static class DataSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var prov = scope.ServiceProvider;

            var logger = prov.GetService<ILoggerFactory>()?.CreateLogger("DataSeed");

            var ctx = prov.GetRequiredService<PetStoreContext>();
            var userMgr = prov.GetRequiredService<UserManager<AppUser>>();
            var roleMgr = prov.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                await ctx.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error applying migrations");
                throw;
            }
            
            var adminRole = "Admin";
            if (!await roleMgr.RoleExistsAsync(adminRole))
            {
                await roleMgr.CreateAsync(new IdentityRole(adminRole));
            }

            var adminEmail = "admin@petstore.com";
            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Site Administrator"
                };

                var result = await userMgr.CreateAsync(admin, "P@ssw0rd!");
                if (result.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, adminRole);
                }
                else
                {
                    logger?.LogWarning("Failed to create admin user: {Errors}", string.Join(',', result.Errors.Select(e => e.Description)));
                }
            }

            // seed initial data
            if (!ctx.Manufacturers.Any())
            {
                Manufacturer m = new()
                {
                    Name = "IgrackeSmoMi",
                    Country = "USA"
                };
                
                ToyCategory category = new()
                {
                    Name = "Dog Toys"
                };
                ToyCategory category2 = new()
                {
                    Name = "Cat Toys"
                };
                ToyCategory category3 = new()
                {
                    Name = "Bird Toys"
                };

                Toy toy = new() {
                    Name = "Squeaky Bone",
                    Description = "Rubber bone",
                    Price = 16.50M,
                    Category = category,
                    Manufacturer = m,
                    QuantityAvailable = 5
                };
                Toy toy2 = new() {
                    Name = "Cat tower",
                    Description = "Durable cat tower - small",
                    Price = 22.95M,
                    Category = category2,
                    Manufacturer = m,
                    QuantityAvailable = 10
                };
                Toy toy3 = new()
                {
                    Name = "Cat tower Large",
                    Description = "Durable cat tower - large",
                    Price = 22.95M,
                    Category = category2,
                    Manufacturer = m,
                    QuantityAvailable = 20
                };
                Toy toy4 = new()
                {
                    Name = "Bird Swing",
                    Description = "Colourful bird swing",
                    Price = 9.99M,
                    Category = category3,
                    Manufacturer = m,
                    QuantityAvailable = 30
                };

                User user = new()
                {
                    FullName = "Dino NN",
                    Email = "dino@gmail.com",
                    Username = "dino",
                    Address = "Grbavička 30, Novo Sarajevo",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                PurchaseDetail purchaseDetail = new()
                {
                    Quantity = 2,
                    Toy = toy,

                };
                PurchaseDetail purchaseDetail2 = new()
                {
                    Quantity = 1,
                    Toy = toy2,
                };
                PurchaseDetail purchaseDetail3 = new()
                {
                    Quantity = 3,
                    Toy = toy4,
                };
                PurchaseDetail purchaseDetail4 = new()
                {
                    Quantity = 1,
                    Toy = toy3,
                };

                Purchase purchase = new()
                {
                    CreditCardNumber = "1234-5678-9012-3456",
                    User = user,
                    PurchaseDate = DateTime.UtcNow,
                    Address = "Konjicka 30, Sarajevo",
                    PurchaseDetails =
                    [
                        purchaseDetail,
                        purchaseDetail2
                    ]
                };

                Purchase purchase2 = new() {
                    CreditCardNumber = "1234-2222-333-3456",
                    User = user,
                    PurchaseDate = DateTime.UtcNow,
                    Address = "Konjicka 30, Sarajevo",
                    PurchaseDetails = [purchaseDetail3, purchaseDetail4 ]
                };

                ctx.Purchases.Add(purchase);
                ctx.Purchases.Add(purchase2);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
