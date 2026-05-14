using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Services.Interfaces
{
    public interface ICartService
    {
       
        Task<bool> AddToCartAsync(string userId, OrderItemRequestDTO request);

        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId);

        Task<bool> RemoveFromCartAsync(string userId, int productId);
        Task<bool> UpdateQuantityAsync(string userId, int productId, int newQuantity);
        Task<bool> ClearCartAsync(string userId);
    }
}