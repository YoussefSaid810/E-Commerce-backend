using System;

namespace Ecommerce.Core.Models
{
    public class Product
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "EGP";
        public int Stock { get; set; }
        public bool StockTracked { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public Guid? CategoryId { get; set; }
        public decimal? Weight { get; set; }
        // Use explicit fields for dimensions rather than a list
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // For flexible metadata keep it as JSON string
        public string? MetaDataJson { get; set; }
    }
}


//ProductId(GUID)

//Name(string)

//Slug(string, unique)

//Description(text)

//ShortDescription

//Price(decimal(18, 2))

//Currency(string e.g., "EGP", "USD") or normalize to single currency

//Stock (int) — current stock quantity

//StockTracked (bool)

//IsActive (bool)

//CategoryId (FK)

//Weight, Dimensions (optional)

//CreatedAt, UpdatedAt, PublishedAt

//Metadata / JSON field for flexible attribute