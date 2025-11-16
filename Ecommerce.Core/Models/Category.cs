using System;

namespace Ecommerce.Core.Models
{
    public class Category
    {
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Slug { get; set; }
        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
