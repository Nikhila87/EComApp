using EComAPI.Data;
using EComAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EComAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = "Tina"; // Replace with JWT extraction in real app
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId && o.PaymentStatus == "Paid")
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Select(o => new
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Items = o.Items.Select(oi => new
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ImageUrl = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        Price = oi.Product.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        //[HttpPost("place-order")]
        //public async Task<IActionResult> PlaceOrder()
        //{
        //    var userId = "Tina";  // Replace with dynamic userId (e.g., from JWT or logged-in user)

        //    // Fetch CartItems with related Product details
        //    var cartItems = await _context.CartItems
        //        .Include(ci => ci.Product)      // Include Product details in CartItems
        //        .Where(ci => ci.UserId == userId)
        //        .ToListAsync();

        //    if (!cartItems.Any())
        //        return BadRequest("Cart is empty.");

        //    // Create OrderItems for the Order
        //    var orderItems = cartItems.Select(item => new OrderItem
        //    {
        //        ProductId = item.ProductId,
        //        ProductName = item.Product.Name,
        //        ImageUrl = item.Product.ImageUrl,
        //        Price = item.Product.Price,
        //        Quantity = item.Quantity
        //    }).ToList();

        //    // Calculate total amount
        //    decimal totalAmount = orderItems.Sum(item => item.Price * item.Quantity);

        //    // Create a new Order
        //    var order = new Order
        //    {
        //        UserId = userId,
        //        OrderDate = DateTime.UtcNow,
        //        TotalAmount = totalAmount,
        //        OrderItems = orderItems
        //    };

        //    // Add the new Order to the context
        //    _context.Orders.Add(order);

        //    // Remove CartItems (empty the cart after placing order)
        //    _context.CartItems.RemoveRange(cartItems);

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Order placed successfully", orderId = order.Id });
        //}
    }
}
