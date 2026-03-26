using Microsoft.AspNetCore.Identity;

namespace Pet_Store.Models
{
    public class AppUser :IdentityUser
    {
        public string? FullName { get; set; }
    }
}
