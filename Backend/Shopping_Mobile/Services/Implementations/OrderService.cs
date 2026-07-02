using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class OrderService(IUnitOfWork unitOfWork, IMapper mapper) : IOrderService
    {
        public async Task<List<OrderResponseDTO>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            // Trả về DTO thay vì Entity
            return mapper.Map<List<OrderResponseDTO>>(orders.ToList());
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrderAsync()
        {
            var orders = await unitOfWork.Orders.GetAllOrdersAsync();
            return mapper.Map<IEnumerable<OrderResponseDTO>>(orders);
        }

        public async Task UpdateOrderStatusAsync(int orderId, int status)
        {
            var order = await unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Không tìm thấy đơn hàng này!");

            order.Status = status;
            unitOfWork.Orders.Update(order);

            var result = await unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new Exception("Cập nhật trạng thái thất bại.");
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(string userId, OrderRequestDTO request)
        {
            // Kiểm tra đầu vào cơ bản
            if (request?.Items == null || !request.Items.Any())
                throw new ArgumentException("Đơn hàng không có sản phẩm!");

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            // Kiểm tra tồn kho và chuẩn bị dữ liệu
            foreach (var item in request.Items)
            {
                var product = await unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Sản phẩm ID {item.ProductId} không tồn tại.");

                // Kiểm tra tồn kho trực tiếp tại Service
                if (product.Stock < item.Quantity)
                    throw new ArgumentException($"Sản phẩm '{product.ProductName}' không đủ tồn kho.");

                product.Stock -= item.Quantity;
                unitOfWork.Products.Update(product);

                totalAmount += product.Price * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            // Khởi tạo thực thể Order bằng AutoMapper
            var newOrder = mapper.Map<Order>(request);
            newOrder.UserId = userId;
            newOrder.OrderDate = DateTime.Now;
            newOrder.TotalAmount = totalAmount;

            string paymentMethod = string.IsNullOrEmpty(request.PaymentMethod) ? "COD" : request.PaymentMethod;
            // Gán trạng thái dựa trên phương thức thanh toán
            newOrder.Status = (paymentMethod == "COD") ? (int)OrderStatus.Pending : (int)OrderStatus.Paid;

            // Khởi tạo Bill (Hóa đơn)
            var newBill = new Bill
            {
                CreatedDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentMethod = paymentMethod,
                Order = newOrder
            };

            // 5. Lưu toàn bộ thông qua UnitOfWork (Đảm bảo tính Atomic Transaction)
            await unitOfWork.Orders.CreateOrderAsync(newOrder, orderDetails, newBill);

            var result = await unitOfWork.CompleteAsync();

            if (result > 0)
                return mapper.Map<OrderResponseDTO>(newOrder);

            throw new Exception("Lỗi hệ thống khi lưu đơn hàng.");
        }
    }
}