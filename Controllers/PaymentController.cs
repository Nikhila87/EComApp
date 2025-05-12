using EComAPI.Data;
using EComAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace EComAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
       

        private readonly AppDbContext _context;
        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("payment")]
        public IActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            var userId ="Tina"; // or extract from JWT
            var totalAmount = request.TotalAmount;
            var shippingAddress = request.ShippingAddress;
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(request.TotalAmount * 100), // Stripe expects cents
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Your Order"
                        }
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = "http://localhost:4200/payment-success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:4200/payment-cancel"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            var cartItems = _context.CartItems
        .Include(item => item.Product) // Ensure Product is loaded
        .Where(item => item.UserId == userId) // Make sure to filter by the current user
        .ToList();

            var orderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ImageUrl = item.Product.ImageUrl,
                Quantity = item.Quantity,
                Price = item.Product.Price // Save the price here
            }).ToList();

            var order = new Order
            {
                UserId = "Tina", // from JWT if applicable
                StripeSessionId = session.Id,
                TotalAmount = totalAmount,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                FullName = shippingAddress.FullName,
                Street = shippingAddress.Street,
                City = shippingAddress.City,
                ZipCode = shippingAddress.ZipCode,
                Country = shippingAddress.Country,
                Items = orderItems
            };

            _context.Orders.Add(order);
             _context.SaveChangesAsync();
            return Ok(new { sessionUrl = session.Url });
        }

        [HttpGet("order/{sessionId}")]
        public async Task<IActionResult> GetOrderDetails(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            // Optionally map session info to your Order DB entry
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);
            if (session.PaymentStatus == "paid")
            {
                order.PaymentStatus = "Paid";
                await _context.SaveChangesAsync();
            }
            return Ok(order);
        }

    }

    public class PaymentRequest
    {
        public decimal TotalAmount { get; set; }
        public Address ShippingAddress { get; set; }
    }

}
