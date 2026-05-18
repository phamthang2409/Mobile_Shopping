using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(string userId, OrderItemRequestDTO request);
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId);
        Task UpdateQuantityAsync(string userId, int productId, int newQuantity);
        Task RemoveFromCartAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}   