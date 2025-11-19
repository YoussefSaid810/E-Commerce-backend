using System.Collections.Generic;

namespace Ecommerce.Application.DTO.Product
{
    public class ProductSearchResultDto
    {
        public long Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ProductListItemDto> Items { get; set; } = new();
    }
}
