using Microsoft.EntityFrameworkCore;

using EComAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EComAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> User  { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Address> Addresses { get; set; }
  
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order>Orders { get; set; }

        public static implicit operator AppDbContext(UserManager<User> v)
        {
            throw new NotImplementedException();
        }
    }
}
