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
        public class ProductReviewController : ControllerBase
        {
            private readonly AppDbContext _context;
            //private readonly UserManager<IdentityUser> _userManager;

            public ProductReviewController(AppDbContext context)
            {
                _context = context;
                //_userManager = userManager;
            }

            [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddReview(ProductReview review)
            {
                var userId = User.FindFirstValue(ClaimTypes.Name);
                review.UserId = userId;
                review.CreatedAt = DateTime.UtcNow;
            

                _context.ProductReviews.Add(review);
                await _context.SaveChangesAsync();

            var productReviews = await _context.ProductReviews
    .Where(r => r.ProductId == review.ProductId)
    .ToListAsync();

            var averageRating = productReviews.Average(r => r.Rating);

            // Update the product
            var product = await _context.Products.FindAsync(review.ProductId);
            if (product != null)
            
                product.averageratings = (int)Math.Round(averageRating, 1); // round to 1 decimal if you want
                await _context.SaveChangesAsync();
            
            return Ok(review);


            }

            [HttpGet("{productId}")]
            public async Task<IActionResult> GetReviews(int productId)
            {
                var reviews = await _context.ProductReviews
                                    .Where(r => r.ProductId == productId)
                                    .OrderByDescending(r => r.CreatedAt)
                                    .ToListAsync();

                return Ok(reviews);
            }
        }

    }

