using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
            // Code xử lý context đã nằm ở lớp cha (base)
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            return await _context.Products
                .Where(p => p.ProductName.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity) return false;

            product.Stock -= quantity;
            return true;
        }
    }
}