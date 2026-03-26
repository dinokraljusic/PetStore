using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pet_Store.Data;
using Pet_Store.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<PetStoreContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PetStoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PetStoreContext") ??
        throw new InvalidOperationException("Connection string 'PetStoreContext' not found.")
    )
 );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// seed data and admin if no data in the DB:
await DataSeed.SeedAsync(app.Services);

app.Run();
