using Microsoft.AspNetCore.Mvc;
using Pet_Store.Models;
using System.Diagnostics;
using Pet_Store.Data;
using Microsoft.EntityFrameworkCore;
using Pet_Store.ViewModels;

namespace Pet_Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly PetStoreContext _context;

        public HomeController(PetStoreContext ctx)
        {
            _context = ctx;
        }

        public async Task<IActionResult> Index(int? categoryId, int? manufacturerId, bool? success)
        {
            if (success != null && success == true)
            {
                ViewBag.SuccessMessage = "Purchase successful!";
            }
            var vm = new HomeIndexViewModel();

            var toysQuery = _context.Toys.Include(t => t.Category).Include(t => t.Manufacturer).AsQueryable();

            if (categoryId.HasValue)
            {
                if (categoryId.Value == -1)
                {
                    // show toys without category
                    toysQuery = toysQuery.Where(t => t.Category == null);
                }
                else
                {
                    toysQuery = toysQuery.Where(t => t.Category != null && t.Category.Id == categoryId.Value);
                }
            }

            if (manufacturerId.HasValue)
            {
                if (manufacturerId.Value == -1)
                {
                    toysQuery = toysQuery.Where(t => t.Manufacturer == null);
                }
                else
                {
                    toysQuery = toysQuery.Where(t => t.ManufacturerId == manufacturerId.Value);
                }
            }

            vm.Toys = await toysQuery.ToListAsync();
            vm.Categories = await _context.ToyCategories.ToListAsync();
            vm.Manufacturers = await _context.Manufacturers.ToListAsync();


            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> StartPurchase([FromForm] int[] selectedToyIds)
        {
            if (selectedToyIds == null || selectedToyIds.Length == 0)
            {
                TempData["Error"] = "Select at least one toy.";
                return RedirectToAction(nameof(Index));
            }

            var toys = await _context.Toys
                .Where(t => selectedToyIds.Contains(t.Id))
                .AsNoTracking()
                .ToListAsync();

            var vm = new PurchaseCreateViewModel();
            vm.Items = toys.Select(t => new PurchaseItemViewModel
            {
                ToyId = t.Id,
                Name = t.Name,
                Price = t.Price,
                QuantityAvailable = t.QuantityAvailable,
                Quantity = 1
            }).ToList();

            return View("CreatePurchase", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase(PurchaseCreateViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "No items selected.");
                return View("CreatePurchase", model);
            }
            // of course, real credit card validation would be more complex and involve external services (at lease on the BE)
            if (model.CreditCardNumber == null || model.CreditCardNumber.Length < 12)
            {
                ModelState.AddModelError(string.Empty, "Invalid credit card number.");
                return View("CreatePurchase", model);
            }

            var requested = model.Items.GroupBy(i => i.ToyId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var toys = await _context.Toys.Where(t => requested.Keys.Contains(t.Id)).ToDictionaryAsync(t => t.Id);

            // availability validation
            foreach (var kv in requested)
            {
                if (!toys.TryGetValue(kv.Key, out var toy))
                {
                    ModelState.AddModelError(string.Empty, $"Toy (id={kv.Key}) not found.");
                    continue;
                }

                // unavailable
                if (toy.QuantityAvailable == 0)
                {
                    ModelState.AddModelError(string.Empty, $"Toy '{toy.Name}' is out of stock.");
                }
                else if (toy.QuantityAvailable > 0 && kv.Value > toy.QuantityAvailable)
                {
                    // cannot order more than available
                    ModelState.AddModelError(string.Empty, $"Requested quantity for '{toy.Name}' ({kv.Value}) exceeds available stock ({toy.QuantityAvailable}).");
                }
            }

            if (!ModelState.IsValid)
            {
                // re-populate per-item availability display values
                foreach (var it in model.Items)
                {
                    if (toys.TryGetValue(it.ToyId, out var t))
                        it.QuantityAvailable = t.QuantityAvailable;
                }
                return View("CreatePurchase", model);
            }

            // perform atomic decrement inside a transaction to avoid race conditions
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // attempt conditional updates for each requested toy (only for toys with limited stock)
                foreach (var kv in requested)
                {
                    var toyId = kv.Key;
                    var reqQty = kv.Value;
                    if (!toys.TryGetValue(toyId, out var toy))
                        continue; // already handled above

                    // unlimited stock (negative) -> don't decrement
                    if (toy.QuantityAvailable < 0)
                        continue;

                    // conditional decrement: only succeed if current available >= requested
                    var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Toys SET QuantityAvailable = QuantityAvailable - {reqQty} WHERE Id = {toyId} AND QuantityAvailable >= {reqQty}");
                    if (rows == 0)
                    {
                        // someone else ordered in the meantime - roll back
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, $"Not enough stock for '{toy.Name}' to fulfill requested quantity ({reqQty}). Try again.");
                        // refresh availability values for display
                        var fresh = await _context.Toys.Where(t => requested.Keys.Contains(t.Id)).ToDictionaryAsync(t => t.Id);
                        foreach (var it in model.Items)
                        {
                            if (fresh.TryGetValue(it.ToyId, out var t)) it.QuantityAvailable = t.QuantityAvailable;
                        }
                        return View("Index");
                    }
                }

                // now that decrements succeeded, create or find user and create purchase rows
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    user = new User { Email = model.Email, Username = model.Name ?? model.Email, FullName = model.Name, CreatedAt = DateTime.UtcNow, Address = model.Address };
                    _context.Users.Add(user);
                }

                var purchaseDetails = new List<PurchaseDetail>();
                // reload toys to get current values and attach to EF
                foreach (var item in model.Items)
                {
                    var toy = await _context.Toys.FindAsync(item.ToyId);
                    if (toy == null) continue;

                    // if QuantityAvailable == 0 skip (shouldn't happen after conditional update)
                    if (toy.QuantityAvailable == 0) continue;

                    var qty = item.Quantity;

                    purchaseDetails.Add(new PurchaseDetail { Toy = toy, Quantity = qty });
                }

                var purchase = new Purchase
                {
                    User = user,
                    PurchaseDate = DateTime.UtcNow,
                    CreditCardNumber = model.CreditCardNumber,
                    Address = string.IsNullOrWhiteSpace(model.Address) ? user.Address : model.Address,
                    PurchaseDetails = purchaseDetails
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index), new { success = true });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ToyCreate()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
