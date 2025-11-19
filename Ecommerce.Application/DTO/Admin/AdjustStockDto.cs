using System;

namespace Ecommerce.Application.DTO.Admin
{
    public class AdjustStockDto
    {
        public Guid ProductId { get; set; }
        public int NewStock { get; set; }
    }
}
