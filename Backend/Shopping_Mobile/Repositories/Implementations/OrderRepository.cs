using Shopping_Mobile.Data;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Mobile.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateOrderAsync(Order order, List<OrderDetail> orderDetails, Bill bill)
        {
            try
            {
                order.OrderDetails = orderDetails;
                order.Bill = bill;
                await _context.Orders.AddAsync(order);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Bill)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Bill)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Bill)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
        }
        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }

        public void Update(Order order)
        {
            _context.Orders.Update(order);
        }
    }
}