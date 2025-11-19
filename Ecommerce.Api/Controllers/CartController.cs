using Ecommerce.Application.DTOs.Cart;
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
    [Route("api/cart")]
    [Authorize] // remove for testing if needed
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CartController(AppDbContext db) => _db = db;

        private string GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User id not found");

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Ok(new CartDto { CartId = Guid.Empty, UserId = userId });

            var resp = new CartDto { CartId = cart.CartId, UserId = cart.UserId };
            foreach (var i in cart.Items ?? Enumerable.Empty<CartItem>())
            {
                var p = await _db.Products.FindAsync(i.ProductId);
                resp.Items.Add(new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    //ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ProductName = p?.Name,
                    //Sku = i.Sku
                });
            }
            return Ok(resp);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto req)
        {
            var userId = GetUserId();

            if (req.Quantity <= 0) req.Quantity = 1;

            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var existing = cart.Items?.FirstOrDefault(i =>
                i.ProductId == req.ProductId);// && i.ProductVariantId == req.ProductVariantId);

            if (existing != null)
            {
                existing.Quantity += req.Quantity;
            }
            else
            {
                // resolve price (variant price or product price)
                decimal unitPrice = 0m;
                //if (req.ProductVariantId.HasValue)
                //{
                //    var variant = await _db.ProductVariants.FindAsync(req.ProductVariantId.Value);
                //    if (variant != null && variant.Price.HasValue) unitPrice = variant.Price.Value;
                //}
                if (unitPrice == 0m)
                {
                    var product = await _db.Products.FindAsync(req.ProductId);
                    if (product == null) return BadRequest("Product not found");
                    unitPrice = product.Price;
                }

                var item = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = req.ProductId,
                    //ProductVariantId = req.ProductVariantId,
                    Quantity = req.Quantity,
                    UnitPrice = unitPrice,
                    AddedAt = DateTime.UtcNow
                };
                _db.CartItems.Add(item);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // return the cart DTO
            var resp = new CartDto { CartId = cart.CartId, UserId = cart.UserId };
            foreach (var i in cart.Items ?? Enumerable.Empty<CartItem>())
            {
                var p = await _db.Products.FindAsync(i.ProductId);
                resp.Items.Add(new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    //ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ProductName = p?.Name,
                    //Sku = i.Sku
                });
            }

            return CreatedAtAction(nameof(GetCart), null, resp);
        }

        [HttpPut("items/{cartItemId:guid}")]
        public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto req)
        {
            var userId = GetUserId();
            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NotFound();

            var item = cart.Items?.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item == null) return NotFound();

            if (req.Quantity <= 0)
                _db.CartItems.Remove(item);
            else
                item.Quantity = req.Quantity;

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // return updated cart DTO
            var resp = new CartDto { CartId = cart.CartId, UserId = cart.UserId };
            foreach (var i in cart.Items ?? Enumerable.Empty<CartItem>())
            {
                var p = await _db.Products.FindAsync(i.ProductId);
                resp.Items.Add(new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    //ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    ProductName = p?.Name,
                    //Sku = i.Sku
                });
            }

            return Ok(resp);
        }

        [HttpDelete("items/{cartItemId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid cartItemId)
        {
            var userId = GetUserId();
            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NoContent();

            var item = cart.Items?.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item == null) return NoContent();

            _db.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NoContent();

            _db.CartItems.RemoveRange(cart.Items!);
            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
