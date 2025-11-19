using Ecommerce.Application.DTO.Product;
using Ecommerce.Core.Models; // or correct namespace for Product
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/products?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products!.AsNoTracking().Where(p => p.IsActive);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                MetaDataJson = p.MetaDataJson
            }).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // GET api/products/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products!.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == id);
        if (p == null) return NotFound();
        var dto = new ProductDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            MetaDataJson = p.MetaDataJson
        };
        return Ok(dto);
    }

    // POST api/products
    [HttpPost]
    [Authorize(Roles = "Admin")] // remove or change if not using roles
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            Price = dto.Price,
            Stock = dto.Stock,
            StockTracked = dto.StockTracked,
            IsActive = dto.IsActive,
            CategoryId = dto.CategoryId,
            Weight = dto.Weight,
            Length = dto.Length,
            Width = dto.Width,
            Height = dto.Height,
            MetaDataJson = dto.MetaDataJson,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products!.Add(product);
        await _db.SaveChangesAsync();

        var resultDto = new ProductDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            MetaDataJson = product.MetaDataJson
        };

        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, resultDto);
    }

    // PUT api/products/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = await _db.Products!.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();

        // map fields
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.StockTracked = dto.StockTracked;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;
        product.Weight = dto.Weight;
        product.Length = dto.Length;
        product.Width = dto.Width;
        product.Height = dto.Height;
        product.MetaDataJson = dto.MetaDataJson;
        product.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating product {ProductId}", id);
            return Conflict(new { message = "Concurrency conflict while updating product" });
        }

        return NoContent(); // 204
    }

    // DELETE api/products/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _db.Products!.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product == null) return NotFound();

        // soft delete option:
        // product.IsActive = false;
        // product.UpdatedAt = DateTime.UtcNow;
        // await _db.SaveChangesAsync();
        // return NoContent();

        // hard delete:
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>
    /// Search and list products with filters, paging and sort.
    /// Query params:
    /// - q (string) : fulltext-ish search on name + description
    /// - category (string) : category id or slug
    /// - minPrice (decimal)
    /// - maxPrice (decimal)
    /// - sort (string) : price_asc | price_desc | newest | oldest | name_asc | name_desc
    /// - page (int) page number (1)
    /// - pageSize (int) page size (20)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
    [FromQuery] string? q = null,
    [FromQuery] string? category = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] string? sort = "newest",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;

        // base query
        var products = _db.Products.AsQueryable();

        // only active products by default
        products = products.Where(p => p.IsActive);

        // category filter (slug or id) — we try to detect the category and filter by its real id
        if (!string.IsNullOrWhiteSpace(category))
        {
            // try numeric id
            if (int.TryParse(category, out var catInt))
            {
                products = products.Where(p => EF.Property<int>(p, "CategoryId") == catInt);
            }
            else if (Guid.TryParse(category, out var catGuid))
            {
                products = products.Where(p => EF.Property<Guid>(p, "CategoryId") == catGuid);
            }
            else
            {
                // treat as slug or name: lookup category first, then filter by the category's id
                var cat = await _db.Categories.AsNoTracking()
                             .FirstOrDefaultAsync(c => c.Slug == category || c.Name == category);
                if (cat == null)
                {
                    return Ok(new ProductSearchResultDto { Total = 0, Page = page, PageSize = pageSize });
                }

                // determine the category Id property and filter accordingly
                var catIdValue = cat.GetType().GetProperty("Id")?.GetValue(cat);
                if (catIdValue is int intVal)
                    products = products.Where(p => EF.Property<int>(p, "CategoryId") == intVal);
                else if (catIdValue is Guid gVal)
                    products = products.Where(p => EF.Property<Guid>(p, "CategoryId") == gVal);
                else
                    products = products.Where(p => EF.Property<string>(p, "CategoryId") == catIdValue.ToString());
            }
        }

        // text search (case-insensitive using ToLower + LIKE)
        if (!string.IsNullOrWhiteSpace(q))
        {
            var lowered = q.Trim().ToLower();
            products = products.Where(p =>
                (p.Name != null && EF.Functions.Like(p.Name.ToLower(), $"%{lowered}%")) ||
                (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%{lowered}%")) ||
                (p.ShortDescription != null && EF.Functions.Like(p.ShortDescription.ToLower(), $"%{lowered}%"))
            );
        }

        // price range
        if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value);

        // sorting
        products = sort?.ToLower() switch
        {
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            "name_asc" => products.OrderBy(p => p.Name),
            "name_desc" => products.OrderByDescending(p => p.Name),
            "oldest" => products.OrderBy(p => p.CreatedAt),
            _ => products.OrderByDescending(p => p.CreatedAt),
        };

        var total = await products.LongCountAsync();

        var list = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto
            {
                // If your Product uses another primary key name (ProductId etc) replace p.Id with that property.
                Id = p.ProductId.ToString(),
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                Slug = p.Slug,
                Price = p.Price,
                Currency = p.Currency,
                Stock = p.StockTracked ? (int?)p.Stock : null,
                StockTracked = p.StockTracked,
                IsActive = p.IsActive,
                Category = null, // no nav property available
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new ProductSearchResultDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = list
        };

        return Ok(result);
    }

}
