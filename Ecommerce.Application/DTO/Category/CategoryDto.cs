using System;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
