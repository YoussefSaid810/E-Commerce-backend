using System;
using System.Collections.Generic;

namespace Ecommerce.Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public string UserId { get; set; } = null!;
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Total => CalculateTotal();
        private decimal CalculateTotal()
        {
            decimal t = 0;
            foreach (var i in Items) t += i.LineTotal;
            return t;
        }
    }
}
