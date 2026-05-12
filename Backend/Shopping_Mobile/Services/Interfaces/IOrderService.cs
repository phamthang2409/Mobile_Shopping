using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
    public interface IOrderService
    {
       
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
        Task<(bool IsSuccess, string Message)> CreateOrderAsync(string userId, OrderRequestDTO request);
        Task<IEnumerable<Order>> GetAllOrderAsync();

    }
}