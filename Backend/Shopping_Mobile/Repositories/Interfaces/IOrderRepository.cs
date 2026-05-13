using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<bool> CreateOrderAsync(Order order, List<OrderDetail> orderDetails, Bill bill);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        void UpdateOrder(Order order);
    }
}