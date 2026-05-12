using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces; 

namespace Shopping_Mobile.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(AppDbContext context, IOrderRepository orderRepository, IMapper mapper)
        {
            _context = context;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        // 1. THAY THẾ GetCart BẰNG LẤY LỊCH SỬ ĐƠN HÀNG
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            // Lấy danh sách đơn hàng kèm theo Chi tiết sản phẩm và Hóa đơn (Bill)
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Bill)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // ĐÃ XÓA: Hàm ProcessCheckoutAsync vì không còn dùng bảng Cart/CartItems nữa.

        public async Task<IEnumerable<Order>> GetAllOrderAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Bill)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // 2. LOGIC CHECKOUT MỚI: Tính tiền từ DB, Tạo Bill, Gọi Repository
        public async Task<(bool IsSuccess, string Message)> CreateOrderAsync(string userId, OrderRequestDTO request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return (false, "Đơn hàng không có sản phẩm hoặc dữ liệu không hợp lệ!");

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            // Lấy giá chuẩn từ Database (Bảo mật: KHÔNG dùng x.Price từ DTO do Frontend gửi lên)
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return (false, $"Sản phẩm ID {item.ProductId} không tồn tại.");

                totalAmount += product.Price * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price // Gán giá gốc từ DB
                });
            }

            // Tạo đối tượng Order
            var newOrder = new Order
            {
                UserId = userId, // Lưu ý: userId ở đây là string theo chuẩn Controller mới
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                Note = request.Note,
                Status = 0 // 0: Đơn mới chờ xử lý
            };

            // Tạo đối tượng Bill kèm theo
            string paymentMethod = string.IsNullOrEmpty(request.PaymentMethod) ? "COD" : request.PaymentMethod;
            var newBill = new Bill
            {
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod
            };

            try
            {
                // Giao việc lưu Transaction xuống Repository
                await _orderRepository.CreateOrderAsync(newOrder, orderDetails, newBill);

                return (true, "Đặt hàng và xuất hóa đơn thành công!");
            }
            catch (Exception)
            {
                // Có thể dùng ILogger để log lỗi chi tiết tại đây nếu cần
                return (false, "Lỗi hệ thống khi lưu đơn hàng. Vui lòng thử lại sau.");
            }
        }
    }
}