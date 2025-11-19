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
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }




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

            builder.Entity<Cart>().HasKey(c => c.CartId);
            builder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique(true); // one cart per user

            builder.Entity<CartItem>().HasKey(ci => ci.CartItemId);
            builder.Entity<CartItem>().HasIndex(ci => new { ci.CartId, ci.ProductId}).IsUnique(false);

            builder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<Order>(b =>
            {
                b.HasKey(o => o.OrderId);
                b.Property(o => o.Total).HasPrecision(18, 2);
                b.Property(o => o.Currency).HasMaxLength(10);
                b.HasMany(o => o.Items)
                 .WithOne(i => i.Order)
                 .HasForeignKey(i => i.OrderId);
            });

            builder.Entity<OrderItem>(b =>
            {
                b.HasKey(i => i.OrderItemId);
                b.Property(i => i.UnitPrice).HasPrecision(18, 2);
                b.Property(i => i.LineTotal).HasPrecision(18, 2);
            });


            // Name / Slug index for faster lookup by slug or name
            builder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            builder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .HasDatabaseName("IX_Products_Slug");

            // Price index for range queries
            builder.Entity<Product>()
                .HasIndex(p => p.Price)
                .HasDatabaseName("IX_Products_Price");

            // CreatedAt index for sorting by newest/oldest
            builder.Entity<Product>()
                .HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Products_CreatedAt");
        }

    }
}
