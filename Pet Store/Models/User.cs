namespace Pet_Store.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public enum UserRole
    {
        Admin,
        Staff,
        Customer
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public string? FullName { get; set; }

        public string? Address { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public ICollection<Purchase> Purchases { get; set; } = null!;
    }
}
