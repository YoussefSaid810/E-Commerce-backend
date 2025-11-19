using System;

namespace Ecommerce.Application.DTOs.Cart
{
    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        //public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
        public string? ProductName { get; set; }
        //public string? Sku { get; set; }    
    }
}
