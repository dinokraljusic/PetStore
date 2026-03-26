using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pet_Store.Data;
using Pet_Store.Models;
using Pet_Store.ViewModels;

namespace Pet_Store.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly PetStoreContext _context;

        public AdminController(PetStoreContext ctx)
        {
            _context = ctx;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                Purchases = await _context.Purchases
                    .Include(p => p.User)
                    .Include(p => p.PurchaseDetails)
                        .ThenInclude(d => d.Toy)
                            .ThenInclude(t => t.Manufacturer)
                    .ToListAsync(),
                Manufacturers = await _context.Manufacturers.ToListAsync(),
                Categories = await _context.ToyCategories.ToListAsync(),
                Toys = await _context.Toys.Include(t => t.Manufacturer).Include(t => t.Category).ToListAsync()
            };

            return View(vm);
        }

        // GET: /Admin/CreateToy or /Admin/CreateToy/{id}
        public async Task<IActionResult> CreateToy(int? id)
        {
            ViewData["Manufacturers"] = await _context.Manufacturers.ToListAsync();
            ViewData["Categories"] = await _context.ToyCategories.ToListAsync();

            if (!id.HasValue)
            {
                return View(new Toy());
            }

            var toy = await _context.Toys.Include(t => t.Manufacturer).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id.Value);
            if (toy == null) return NotFound();
            return View(toy);
        }

        public async Task<IActionResult> DeleteToy(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            var toy = await _context.Toys.FirstOrDefaultAsync(t => t.Id == id.Value);
            if (toy == null) return NotFound();
            _context.Toys.Remove(toy);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/CreateToy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateToy(Toy toy, int? manufacturerId, int? categoryId)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Manufacturers"] = await _context.Manufacturers.ToListAsync();
                ViewData["Categories"] = await _context.ToyCategories.ToListAsync();
                return View(toy);
            }

            // set manufacturer/category references if provided
            Manufacturer? m = null;
            ToyCategory? c = null;
            if (manufacturerId.HasValue)
            {
                m = await _context.Manufacturers.FindAsync(manufacturerId.Value);
            }
            if (categoryId.HasValue)
            {
                c = await _context.ToyCategories.FindAsync(categoryId.Value);
            }

            if (toy.Id > 0)
            {
                var existing = await _context.Toys.Include(t => t.Manufacturer).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == toy.Id);
                if (existing == null) return NotFound();
                existing.Name = toy.Name;
                existing.Description = toy.Description;
                existing.Price = toy.Price;
                existing.Manufacturer = m;
                existing.ManufacturerId = m?.Id;
                existing.Category = c;
                existing.QuantityAvailable = toy.QuantityAvailable;
                _context.Toys.Update(existing);
            }
            else
            {
                toy.Manufacturer = m;
                toy.Category = c;
                _context.Toys.Add(toy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/CreateManufacturer or /Admin/CreateManufacturer/{id}
        public async Task<IActionResult> CreateManufacturer(int? id)
        {
            if (!id.HasValue)
            {
                return View(new Manufacturer());
            }

            var m = await _context.Manufacturers.FindAsync(id.Value);
            if (m == null) return NotFound();
            return View(m);
        }

        // POST: /Admin/CreateManufacturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManufacturer(Manufacturer manufacturer)
        {
            if (!ModelState.IsValid) return View(manufacturer);

            if (manufacturer.Id > 0)
            {
                var existing = await _context.Manufacturers.FindAsync(manufacturer.Id);
                if (existing == null) return NotFound();
                existing.Name = manufacturer.Name;
                existing.Country = manufacturer.Country;
                _context.Manufacturers.Update(existing);
            }
            else
            {
                _context.Manufacturers.Add(manufacturer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/CreateCategory or /Admin/CreateCategory/{id}
        public async Task<IActionResult> CreateCategory(int? id)
        {
            if (!id.HasValue)
            {
                return View(new ToyCategory());
            }

            var category = await _context.ToyCategories.FindAsync(id.Value);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(ToyCategory category)
        {
            if (!ModelState.IsValid) return View(category);

            if (category.Id > 0)
            {
                var existing = await _context.ToyCategories.FindAsync(category.Id);
                if (existing == null) return NotFound();
                existing.Name = category.Name;
                _context.ToyCategories.Update(existing);
            }
            else
            {
                _context.ToyCategories.Add(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
