using System;
using System.ComponentModel.DataAnnotations;

public class CategoryUpdateDto
{
    [Required]
    public string Name { get; set; } = null!;

    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
