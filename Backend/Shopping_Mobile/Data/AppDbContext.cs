using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // THÊM: Bảng Bill để lưu hóa đơn
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //  1-1 giữa Order và Bill
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Bill)
                .WithOne(b => b.Order)
                .HasForeignKey<Bill>(b => b.OrderId);

            base.OnModelCreating(modelBuilder);
        }
    }
}