using System;

namespace Ecommerce.Application.DTOs.Cart
{
    public class AddCartItemDto
    {
        public Guid ProductId { get; set; }
        //public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
