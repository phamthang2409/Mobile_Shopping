using Shopping_Mobile.Data;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Repositories.Implementations;

namespace Shopping_Mobile.Repositories.Implementations
{
    public class CartRepository : GenericRepository<CartItem>, ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CartItem?> GetItemInCartAsync(string userId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToListAsync();
        }

        public void ClearCart(string userId)
        {
            var items = _context.CartItems.Where(c => c.UserId == userId);
            _context.CartItems.RemoveRange(items);
        }
    }
}