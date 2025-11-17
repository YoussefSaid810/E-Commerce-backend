using Ecommerce.Infrastructure.Data;
using Ecommerce.Core.Models;
using Ecommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/products/{productId:guid}/images")]
public class ProductImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly ILogger<ProductImagesController> _logger;
    private const string RelativeFolder = "uploads/products";
    private static readonly string[] AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

    public ProductImagesController(AppDbContext db, IFileStorageService storage, ILogger<ProductImagesController> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    // GET: api/products/{productId}/images
    [HttpGet]
    public async Task<IActionResult> Get(Guid productId)
    {
        var exists = await _db.Products.AnyAsync(p => p.ProductId == productId);
        if (!exists) return NotFound(new { message = "Product not found" });

        var images = await _db.ProductImages
            .Where(pi => pi.ProductId == productId)
            .OrderByDescending(pi => pi.IsPrimary)
            .ThenBy(pi => pi.SortOrder)
            .Select(pi => new {
                pi.ProductImageId,
                Url = _storage.GetPublicUrl(pi.FileName),
                pi.Caption,
                pi.IsPrimary,
                pi.SortOrder,
                pi.CreatedAt
            }).ToListAsync();

        return Ok(images);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(Guid productId, [FromForm(Name = "file")] IFormFile file, [FromForm] string? caption)
    {
        if (file == null) return BadRequest("file is required");
        if (!AllowedContentTypes.Contains(file.ContentType)) return BadRequest(new { message = "Unsupported image type" });
        if (file.Length <= 0 || file.Length > MaxFileBytes) return BadRequest(new { message = $"File must be >0 and <= {MaxFileBytes} bytes" });

        var product = await _db.Products.FindAsync(productId);
        if (product == null) return NotFound(new { message = "Product not found" });

        // Save file to disk
        var relativePath = await _storage.SaveFileAsync(file, RelativeFolder); // e.g. uploads/products/xxx.jpg

        var pi = new ProductImage
        {
            ProductId = productId,
            FileName = relativePath, // store relative web path
            Caption = caption,
            CreatedAt = DateTime.UtcNow
        };

        // if no other images, mark primary
        var any = await _db.ProductImages.AnyAsync(x => x.ProductId == productId);
        if (!any) pi.IsPrimary = true;

        _db.ProductImages.Add(pi);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { productId }, new
        {
            pi.ProductImageId,
            Url = _storage.GetPublicUrl(pi.FileName),
            pi.IsPrimary,
            pi.Caption,
            pi.SortOrder
        });
    }

    // DELETE: api/products/{productId}/images/{imageId}
    [HttpDelete("{imageId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid productId, Guid imageId)
    {
        var pi = await _db.ProductImages.FirstOrDefaultAsync(x => x.ProductImageId == imageId && x.ProductId == productId);
        if (pi == null) return NotFound();

        // delete physical file
        try
        {
            await _storage.DeleteFileAsync(pi.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file {File}", pi.FileName);
            // continue — still remove DB record
        }

        _db.ProductImages.Remove(pi);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // You can add endpoints to set IsPrimary, reorder, etc.
}
