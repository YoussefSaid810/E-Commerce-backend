using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models
{
    public class Cart
    {
        public Guid CartId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem>? Items { get; set; }
    }
}
