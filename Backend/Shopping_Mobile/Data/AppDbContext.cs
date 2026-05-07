using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Models;
using System.Collections.Generic;

namespace Shopping_Mobile.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Product> Products { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}   