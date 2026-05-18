using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderResponseDTO>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<OrderResponseDTO>> GetAllOrderAsync();
        Task UpdateOrderStatusAsync(int orderId, int status);
        Task<OrderResponseDTO> CreateOrderAsync(string userId, OrderRequestDTO request);
    }
}