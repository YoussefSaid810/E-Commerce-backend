using System;
using System.Collections.Generic;

namespace Ecommerce.Application.DTO.Order
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public string UserId { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal Total { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public string? Notes { get; set; }
    }
}
