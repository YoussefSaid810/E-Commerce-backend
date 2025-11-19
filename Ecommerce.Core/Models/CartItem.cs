using System;

namespace Ecommerce.Core.Models
{
    public class CartItem
    {
        public Guid CartItemId { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        //public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public string? ProductName { get; set; }
        //public string? Sku { get; set; }
    }
}
