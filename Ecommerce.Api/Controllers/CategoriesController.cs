using Ecommerce.Core.Models;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(AppDbContext db, ILogger<CategoriesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/categories?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeInactive = false)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Categories!.AsNoTracking()
                .Where(c => includeInactive || c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // GET: api/categories/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await _db.Categories!.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryId == id);
        if (c == null) return NotFound();
        var dto = new CategoryDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            Slug = c.Slug,
            ParentId = c.ParentId,
            SortOrder = c.SortOrder,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
        return Ok(dto);
    }

    // POST: api/categories
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // ensure slug unique if provided, or generate
        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug!.Trim();
        var exists = await _db.Categories!.AnyAsync(x => x.Slug == slug);
        if (exists) return Conflict(new { message = "Slug already in use" });

        var cat = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = dto.Name,
            Slug = slug,
            ParentId = dto.ParentId,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        var result = new CategoryDto
        {
            CategoryId = cat.CategoryId,
            Name = cat.Name,
            Slug = cat.Slug,
            ParentId = cat.ParentId,
            SortOrder = cat.SortOrder,
            IsActive = cat.IsActive,
            CreatedAt = cat.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = cat.CategoryId }, result);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var cat = await _db.Categories!.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug!.Trim();
        var exists = await _db.Categories.AnyAsync(x => x.Slug == slug && x.CategoryId != id);
        if (exists) return Conflict(new { message = "Slug already in use by another category" });

        cat.Name = dto.Name;
        cat.Slug = slug;
        cat.ParentId = dto.ParentId;
        cat.SortOrder = dto.SortOrder;
        cat.IsActive = dto.IsActive;
        cat.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cat = await _db.Categories!.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat == null) return NotFound();

        // Soft delete recommended:
        cat.IsActive = false;
        cat.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // simple slug generator
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Guid.NewGuid().ToString("N");
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("/", "-")
            .Replace("\\", "-");
        // remove invalid chars
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // collapse multiple dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }
}
