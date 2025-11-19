using Ecommerce.Application.DTO.Admin;
using Ecommerce.Core.Models;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db) => _db = db;

        // 1) List orders with optional status / date range / paging
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string? status = null,
                                                   [FromQuery] DateTime? from = null,
                                                   [FromQuery] DateTime? to = null,
                                                   [FromQuery] int page = 1,
                                                   [FromQuery] int pageSize = 25)
        {
            var q = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var st))
                q = q.Where(o => o.Status == st);

            if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(o => o.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

            var result = items.Select(o => new
            {
                o.OrderId,
                o.UserId,
                Status = o.Status.ToString(),
                o.Total,
                o.Currency,
                o.CreatedAt,
                ItemCount = o.Items.Count
            });

            return Ok(new { total, page, pageSize, items = result });
        }

        // 2) Change order status (Admin)
        [HttpPost("orders/change-status")]
        public async Task<IActionResult> ChangeOrderStatus([FromBody] ChangeOrderStatusDto dto)
        {
            var order = await _db.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound();

            if (!Enum.TryParse<OrderStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest("Invalid status");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { order.OrderId, status = order.Status.ToString() });
        }

        // 3) Adjust product stock
        [HttpPost("products/adjust-stock")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockDto dto)
        {
            var product = await _db.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound();

            product.Stock = dto.NewStock; // assumes Stock is int
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { product.ProductId, product.Name, product.Stock });
        }

        // 4) Sales summary: total sales, orders count, top 5 products by revenue
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var q = _db.Orders.AsQueryable();
            if (from.HasValue) q = q.Where(o => o.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(o => o.CreatedAt <= to.Value);

            var totalSales = await q.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Delivered)
                                    .SumAsync(o => (decimal?)o.Total) ?? 0m;
            var ordersCount = await q.CountAsync();

            var topProducts = await _db.OrderItems
                .Where(i => (!from.HasValue || i.AddedAt >= from.Value) && (!to.HasValue || i.AddedAt <= to.Value))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            return Ok(new { totalSales, ordersCount, topProducts });
        }
    }
}
