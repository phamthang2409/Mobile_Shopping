using Shopping_Mobile.Repositories.Interfaces; // Namespace chứa IGenericRepository
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<CartItem>
    {
        Task<CartItem?> GetItemInCartAsync(string userId, int productId);
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId);
        void ClearCart(string userId);
    }
}