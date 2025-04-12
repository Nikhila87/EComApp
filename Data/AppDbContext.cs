using Microsoft.EntityFrameworkCore;

using EComAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace EComAPI.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> User  { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

    }
}
