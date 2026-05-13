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
        public async Task<(bool IsSuccess, string Message)> CreateOrderAsync(string userId, OrderRequestDTO request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return (false, "Đơn hàng không có sản phẩm hoặc dữ liệu không hợp lệ!");

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            //  Kiểm tra tồn kho và tính tổng tiền
            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    return (false, $"Sản phẩm ID {item.ProductId} không tồn tại.");

                // Cập nhật Stock ngay trong Memory trước khi tạo Order
                bool stockUpdated = await _unitOfWork.Products.UpdateStockAsync(item.ProductId, item.Quantity);
                if (!stockUpdated)
                    return (false, $"Sản phẩm '{product.ProductName}' không đủ số lượng trong kho.");

                totalAmount += product.Price * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            // Khởi tạo thực thể Order
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Name = request.Name ?? "Khách hàng",
                Address = request.Address ?? string.Empty,
                Phone = request.Phone ?? string.Empty,
                Note = request.Note,
                Status = 0 
            };

            // Khởi tạo thực thể Bill
            string paymentMethod = string.IsNullOrEmpty(request.PaymentMethod) ? "COD" : request.PaymentMethod;
            var newBill = new Bill
            {
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod
            };
            try
            {
                await _unitOfWork.Orders.CreateOrderAsync(newOrder, orderDetails, newBill);

                var result = await _unitOfWork.CompleteAsync();

                if (result > 0)
                    return (true, "Đặt hàng và xuất hóa đơn thành công!");

                return (false, "Không có thay đổi nào được lưu vào hệ thống.");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? $" | Chi tiết: {ex.InnerException.Message}" : "";
                return (false, $"Lỗi hệ thống khi lưu: {ex.Message}{innerMessage}");
            }
        }
    }
}