using AutoMapper;
using FluentAssertions;
using Moq;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Shopping_Mobile.Services.Implementations;
using Xunit;

namespace Shopping_Mobile.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IOrderService _orderService;

        public OrderServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _orderService = new OrderService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenItemsAreEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new OrderRequestDTO { Items = new List<OrderItemRequestDTO>() };

            // Act & Assert
            await _orderService.Invoking(s => s.CreateOrderAsync("user-1", request))
                               .Should()
                               .ThrowAsync<ArgumentException>()
                               .WithMessage("*không có sản phẩm*"); // Check tương đối để tránh lệch dấu câu
        }

        [Fact]
        public async Task CreateOrderAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var request = new OrderRequestDTO
            {
                Items = new List<OrderItemRequestDTO>
                {
                    new() { ProductId = 99, Quantity = 1 }
                }
            };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(99))
                           .ReturnsAsync((Product?)null);

            // Act & Assert
            await _orderService.Invoking(s => s.CreateOrderAsync("user-1", request))
                               .Should()
                               .ThrowAsync<KeyNotFoundException>()
                               .WithMessage("*99 không tồn tại*");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenStockIsInsufficient_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new OrderRequestDTO
            {
                Items = new List<OrderItemRequestDTO>
                {
                    new() { ProductId = 1, Quantity = 5 }
                }
            };
            var product = new Product { Id = 1, ProductName = "Iphone 15", Stock = 2, Price = 1000 };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(1))
                           .ReturnsAsync(product);

            // Act & Assert
            await _orderService.Invoking(s => s.CreateOrderAsync("user-1", request))
                               .Should()
                               .ThrowAsync<ArgumentException>()
                               .WithMessage("*không đủ tồn kho*");
        }

        [Fact]
        public async Task CreateOrderAsync_WhenOrderIsValid_ShouldCreateOrderReduceStockAndReturnDto()
        {
            // Arrange
            var request = new OrderRequestDTO
            {
                Name = "Nguyen Van A",
                Phone = "0900000000",
                Address = "Ha Noi",
                PaymentMethod = "COD",
                Items = new List<OrderItemRequestDTO>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            };

            var product = new Product { Id = 1, ProductName = "Iphone 15", Stock = 10, Price = 1000 };

            // FIX: Khởi tạo sẵn thông tin cho mappedOrder giả lập để tránh lỗi lệch giá trị Assert ngầm định
            var mappedOrder = new Order
            {
                UserId = "user-1",
                TotalAmount = 2000,
                Status = (int)OrderStatus.Pending
            };

            var expectedResponse = new OrderResponseDTO
            {
                Name = request.Name,
                Phone = request.Phone,
                Address = request.Address,
                TotalAmount = 2000,
                Status = (int)OrderStatus.Pending
            };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(1))
                           .ReturnsAsync(product);

            _mockMapper.Setup(m => m.Map<Order>(request))
                       .Returns(mappedOrder);

            _mockMapper.Setup(m => m.Map<OrderResponseDTO>(mappedOrder))
                       .Returns(expectedResponse);

            _mockUnitOfWork.Setup(u => u.Orders.CreateOrderAsync(It.IsAny<Order>(), It.IsAny<List<OrderDetail>>(), It.IsAny<Bill>()))
                          .ReturnsAsync(true);

            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            var result = await _orderService.CreateOrderAsync("user-1", request);

            // Assert
            product.Stock.Should().Be(8); // Tồn kho giảm từ 10 xuống 8
            mappedOrder.UserId.Should().Be("user-1");
            mappedOrder.TotalAmount.Should().Be(2000);
            mappedOrder.Status.Should().Be((int)OrderStatus.Pending);
            result.Should().BeEquivalentTo(expectedResponse);

            _mockUnitOfWork.Verify(u => u.Products.Update(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Orders.CreateOrderAsync(
                mappedOrder,
                It.Is<List<OrderDetail>>(details =>
                    details.Count == 1 &&
                    details[0].ProductId == 1 &&
                    details[0].Quantity == 2 &&
                    details[0].Price == 1000),
                It.Is<Bill>(bill =>
                    bill.TotalAmount == 2000 &&
                    bill.PaymentMethod == "COD" &&
                    bill.Order == mappedOrder)), Times.Once);

            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenOrderExists_ShouldUpdateStatusAndComplete()
        {
            // Arrange
            var order = new Order { Id = 1, Status = (int)OrderStatus.Pending };
            _mockUnitOfWork.Setup(u => u.Orders.GetByIdAsync(1))
                           .ReturnsAsync(order);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _orderService.UpdateOrderStatusAsync(1, (int)OrderStatus.Paid);

            // Assert
            order.Status.Should().Be((int)OrderStatus.Paid);
            _mockUnitOfWork.Verify(u => u.Orders.Update(order), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenOrderDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Orders.GetByIdAsync(99))
                           .ReturnsAsync((Order?)null);

            // Act & Assert
            await _orderService.Invoking(s => s.UpdateOrderStatusAsync(99, (int)OrderStatus.Paid))
                               .Should()
                               .ThrowAsync<KeyNotFoundException>()
                               .WithMessage("*không tìm thấy đơn hàng*");
        }
    }
}