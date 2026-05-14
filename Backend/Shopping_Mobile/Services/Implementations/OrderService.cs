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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            return orders.ToList();
        }

        public async Task<IEnumerable<Order>> GetAllOrderAsync()
        {
            return await _unitOfWork.Orders.GetAllOrdersAsync();
        }

        // Cập nhật trạng thái đơn hàng (Dành cho Admin)
        public async Task<bool> UpdateOrderStatusAsync(int orderId, int status)
        {
            try
            {
                // Bước 1: Lấy đơn hàng từ Repo (Sơn hãy đảm bảo IOrderRepository đã có GetByIdAsync)
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null) return false;

                // Bước 2: Cập nhật trạng thái
                order.Status = status;

                // Bước 3: Đánh dấu cập nhật và lưu
                _unitOfWork.Orders.Update(order);
                var result = await _unitOfWork.CompleteAsync();

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(bool IsSuccess, string Message)> CreateOrderAsync(string userId, OrderRequestDTO request)
        {
            // Kiểm tra đầu vào
            if (request == null || request.Items == null || !request.Items.Any())
                return (false, "Đơn hàng không có sản phẩm!");

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            // Kiểm tra tồn kho và tính toán chi tiết
            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    return (false, $"Sản phẩm ID {item.ProductId} không tồn tại.");

                // Cập nhật số lượng kho
                bool stockUpdated = await _unitOfWork.Products.UpdateStockAsync(item.ProductId, item.Quantity);
                if (!stockUpdated)
                    return (false, $"Sản phẩm '{product.ProductName}' không đủ tồn kho.");

                totalAmount += product.Price * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            string paymentMethod = string.IsNullOrEmpty(request.PaymentMethod) ? "COD" : request.PaymentMethod;

            //  Khởi tạo thực thể Order 
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Name = request.Name ?? "Khách hàng",
                Address = request.Address ?? string.Empty,
                Phone = request.Phone ?? string.Empty,
                Note = request.Note,
                // Dùng Enum Pending cho COD, Paid cho chuyển khoản
                Status = (paymentMethod == "COD") ? (int)OrderStatus.Pending : (int)OrderStatus.Paid
            };

            // 4. Khởi tạo hóa đơn (Bill)
            var newBill = new Bill
            {
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                Order = newOrder // Gán quan hệ trực tiếp nếu Model hỗ trợ
            };

            try
            {
                // 5. Lưu toàn bộ thông qua UnitOfWork (Đảm bảo tính Transaction)
                await _unitOfWork.Orders.CreateOrderAsync(newOrder, orderDetails, newBill);
                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                    return (true, "Đặt hàng thành công!");

                return (false, "Không thể lưu dữ liệu.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" | Chi tiết: {ex.InnerException.Message}" : "";
                return (false, $"Lỗi hệ thống: {ex.Message}{innerMessage}");
            }
        }
    }
}