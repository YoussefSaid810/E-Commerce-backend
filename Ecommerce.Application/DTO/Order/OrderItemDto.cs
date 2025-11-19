using System;

namespace Ecommerce.Application.DTO.Order
{
    public class OrderItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        //public Guid? ProductVariantId { get; set; }
        public string? ProductName { get; set; }
        //public string? Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
