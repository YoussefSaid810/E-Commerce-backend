using Ecommerce.Core.DTO;
using Ecommerce.Core.Models; // or correct namespace for Product
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

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
    //[Authorize(Roles = "Admin")] // remove or change if not using roles
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
    //[Authorize(Roles = "Admin")]
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
    //[Authorize(Roles = "Admin")]
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
}
