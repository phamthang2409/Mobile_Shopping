using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
    public interface ICartService
    {      
        Task<List<CartItemResponseDTO>> GetCartByUserIdAsync(int userId);
        Task<bool> ProcessCheckoutAsync(int userId, List<CartDTO> items);
        Task<bool> CreateOrderAsync(int userId, OrderRequestDTO request);
        Task<IEnumerable<Order>> GetAllOrderAsync();

    }
}   