using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Core.DTO
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public string? ShortDescription { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public bool StockTracked { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public Guid? CategoryId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        // optional flexible metadata JSON as string
        public string? MetaDataJson { get; set; }
    }

}
