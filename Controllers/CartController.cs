using EComAPI.Data;
using EComAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EComAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        public CartController(AppDbContext context,UserManager<User> userManager)
        {
            _context = context;
            _userManager=userManager;
        }
      
        // Add item to cart
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]

        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto cartItemDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == cartItemDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity
                };

                _context.CartItems.Add(newItem);
            }


            await _context.SaveChangesAsync();
            return Ok("Item added to cart");
        }

        // Get cart items for a specific user
        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCartItemsForUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
           
            var items = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product)
                .ToListAsync();

            return Ok(items);
        }

        // Remove item from cart
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("{id}")]
       
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId);

            if (item == null)
                return NotFound(new { message = "Item not found in your cart" });

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from cart" });
        }

        // Update quantity of item in cart (optional)
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCartItemQuantity(int id, [FromBody] int quantity)
        {

            var userId = User.FindFirstValue(ClaimTypes.Name);

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == id && ci.UserId == userId);

            if (item == null)
                return NotFound("Item not found");

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Ok("Quantity updated");
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            var cart = await _context.CartItems
                .Where(c => c.UserId == userId).ToListAsync();



            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart);
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

    }
}
