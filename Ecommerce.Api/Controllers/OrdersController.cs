using Ecommerce.Application.DTO.Order;
using Ecommerce.Core.Models;
using Ecommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize] // remove for local testing if you prefer
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db) => _db = db;

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User id not found");

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = GetUserId();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return BadRequest("Cart is empty");

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // calculate total and validate stock
                decimal total = 0m;
                string currency = "USD";
                foreach (var cartItem in cart.Items)
                {
                    var product = await _db.Products.FindAsync(cartItem.ProductId);
                    if (product == null) return BadRequest($"Product not found: {cartItem.ProductId}");

                    var unitPrice = cartItem.UnitPrice;
                    total += unitPrice * cartItem.Quantity;

                    // prefer product currency if present
                    if (!string.IsNullOrWhiteSpace(product.Currency)) currency = product.Currency;

                    // handle stock (assumes Product.Stock is an int)
                    if (product.StockTracked)
                    {
                        // if Stock is int property:
                        if (product.Stock < cartItem.Quantity)
                            return BadRequest($"Insufficient stock for product {product.Name}");
                        product.Stock -= cartItem.Quantity;
                    }
                }

                var order = new Order
                {
                    UserId = userId,
                    Total = total,
                    Currency = currency,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Notes = dto.Notes
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                // create order items
                foreach (var cartItem in cart.Items)
                {
                    var product = await _db.Products.FindAsync(cartItem.ProductId);
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        //ProductVariantId = cartItem.ProductVariantId,
                        ProductName = product?.Name,
                        //Sku = cartItem.Sku,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        LineTotal = cartItem.UnitPrice * cartItem.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    _db.OrderItems.Add(orderItem);
                }

                // Simulate payment success (you can replace with real flow)
                order.Status = OrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;

                // clear cart
                _db.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // map to DTO
                var orderDto = new OrderDto
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Status = order.Status.ToString(),
                    Total = order.Total,
                    Currency = order.Currency,
                    CreatedAt = order.CreatedAt,
                    Notes = order.Notes
                };

                var items = await _db.OrderItems.Where(i => i.OrderId == order.OrderId).ToListAsync();
                foreach (var i in items)
                {
                    orderDto.Items.Add(new OrderItemDto
                    {
                        OrderItemId = i.OrderItemId,
                        ProductId = i.ProductId,
                        //ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductName,
                        //Sku = i.Sku,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.LineTotal
                    });
                }

                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderDto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                // consider logging ex
                return StatusCode(500, new { error = "Failed to create order", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            var query = _db.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = orders.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                Status = o.Status.ToString(),
                Total = o.Total,
                Currency = o.Currency,
                CreatedAt = o.CreatedAt,
                Notes = o.Notes
            }).ToList();

            return Ok(new { total, page, pageSize, items = result });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var userId = GetUserId();
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
            if (order == null) return NotFound();

            var dto = new OrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                Total = order.Total,
                Currency = order.Currency,
                CreatedAt = order.CreatedAt,
                Notes = order.Notes
            };

            foreach (var i in order.Items)
            {
                dto.Items.Add(new OrderItemDto
                {
                    OrderItemId = i.OrderItemId,
                    ProductId = i.ProductId,
                    //ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    //Sku = i.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                });
            }

            return Ok(dto);
        }
    }
}
