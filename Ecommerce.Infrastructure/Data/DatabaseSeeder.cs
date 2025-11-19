using Ecommerce.Core.Models;
using Ecommerce.Core.Identity; // adjust if your ApplicationUser namespace differs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseSeeder(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task MigrateAndSeedAsync()
        {
            // Apply pending migrations (safe for dev)
            await _db.Database.MigrateAsync();

            // Seed roles
            var roles = new[] { "ADMIN", "CUSTOMER" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin user
            var adminEmail = "admin@ecommerce.test";
            var adminUserName = "admin";
            var admin = await _userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, "Admin@1234");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "ADMIN");
                }
                else
                {
                    // optionally log errors
                }
            }
            else
            {
                // ensure role
                var inRole = await _userManager.IsInRoleAsync(admin, "ADMIN");
                if (!inRole) await _userManager.AddToRoleAsync(admin, "ADMIN");
            }

            // Seed categories + products only if none exist
            if (!await _db.Categories.AnyAsync())
            {
                var cat1 = new Category { Name = "Electronics", Slug = "electronics", CreatedAt = DateTime.UtcNow };
                var cat2 = new Category { Name = "Home & Kitchen", Slug = "home-kitchen", CreatedAt = DateTime.UtcNow };
                var cat3 = new Category { Name = "Books", Slug = "books", CreatedAt = DateTime.UtcNow };

                await _db.Categories.AddRangeAsync(cat1, cat2, cat3);
                await _db.SaveChangesAsync();

                // sample products
                var p1 = new Product
                {
                    Name = "Wireless Headphones",
                    Description = "Comfortable wireless headphones with long battery life.",
                    ShortDescription = "Wireless Headphones",
                    Slug = "wireless-headphones",
                    Price = 79.99m,
                    Currency = "EGP",
                    Stock = 50, // assumes Stock is int (change if different)
                    StockTracked = true,
                    IsActive = true,
                    CategoryId = cat1.CategoryId, // if category Id is int; adjust if Guid
                    CreatedAt = DateTime.UtcNow
                };

                var p2 = new Product
                {
                    Name = "Ceramic Coffee Mug",
                    Description = "12oz ceramic mug, dishwasher safe.",
                    ShortDescription = "Ceramic Mug 12oz",
                    Slug = "ceramic-coffee-mug",
                    Price = 9.99m,
                    Currency = "EGP",
                    Stock = 150,
                    StockTracked = true,
                    IsActive = true,
                    CategoryId = cat2.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                var p3 = new Product
                {
                    Name = "C# In Depth (4th Edition)",
                    Description = "Programming book for C# developers.",
                    ShortDescription = "C# book",
                    Slug = "csharp-in-depth",
                    Price = 39.50m,
                    Currency = "EGP",
                    Stock = 30,
                    StockTracked = true,
                    IsActive = true,
                    CategoryId = cat3.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };
                var p4 = new Product
                {
                    Name = "C# In Depth (5th Edition)",
                    Description = "Programming book for C# developers.",
                    ShortDescription = "C# book",
                    Slug = "csharp-in-depth",
                    Price = 59.50m,
                    Currency = "EGP",
                    Stock = 20,
                    StockTracked = true,
                    IsActive = true,
                    CategoryId = cat3.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };
                await _db.Products.AddRangeAsync(p1, p2, p3 , p4);
                await _db.SaveChangesAsync();
            }
        }
    }
}
