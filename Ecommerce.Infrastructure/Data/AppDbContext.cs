using Ecommerce.Core.Identity;
using Ecommerce.Core.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Ecommerce.Infrastructure.Data
{
    public class AppDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>().HasKey(p => p.ProductId);
            builder.Entity<Category>().HasKey(c => c.CategoryId);

            // Decimal precision: set scale and precision appropriate for money and dims
            // Price: decimal(18,2) (typical for money)
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Weight and dimensions: choose precision/scale that fits your domain
            // e.g. decimal(10,2) supports up to 99999999.99
            builder.Entity<Product>()
                .Property(p => p.Weight)
                .HasPrecision(10, 2);

            builder.Entity<Product>()
                .Property(p => p.Length)
                .HasPrecision(10, 2);

            builder.Entity<Product>()
                .Property(p => p.Width)
                .HasPrecision(10, 2);

            builder.Entity<Product>()
                .Property(p => p.Height)
                .HasPrecision(10, 2);

            // Optional: index slug uniqueness for Category/Product if needed
            builder.Entity<Category>().HasIndex(c => c.Slug).IsUnique(false); // set true if you want unique
            builder.Entity<Product>().HasIndex(p => p.Slug).IsUnique(false);

            builder.Entity<ProductImage>()
                .HasKey(pi => pi.ProductImageId);
            builder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.IsPrimary });

        }

    }
}
