using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ecommerce.Core.Models
{
    public class ProductImage
    {
        public Guid ProductImageId { get; set; } = Guid.NewGuid();

        // FK to Product
        public Guid ProductId { get; set; }
        // optional navigation property (Core shouldn't depend on Infrastructure)
        // public Product? Product { get; set; } 

        // filename stored in wwwroot/uploads/products
        public string FileName { get; set; } = null!;
        // public url path (can be computed at runtime)
        public string? Caption { get; set; }

        // is primary (main) image
        public bool IsPrimary { get; set; } = false;

        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
