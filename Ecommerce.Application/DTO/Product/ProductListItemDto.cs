using System;

namespace Ecommerce.Application.DTO.Product
{
    public class ProductListItemDto
    {
        public string Id { get; set; } = null!; // string to avoid Guid/int mismatch between projects
        public string Name { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? Slug { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EGP";
        public int? Stock { get; set; }         // nullable in case Stock is not tracked or typed differently
        public bool StockTracked { get; set; }
        public bool IsActive { get; set; }
        public string? Category { get; set; }   
        public DateTime CreatedAt { get; set; }
    }
}
