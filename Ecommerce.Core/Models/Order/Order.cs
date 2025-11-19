using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models
{
    public class Order
    {
        public Guid OrderId { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal Total { get; set; }
        public string Currency { get; set; } = "EGP";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? Notes { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
