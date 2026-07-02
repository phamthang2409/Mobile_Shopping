using FluentAssertions;
using Moq;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Shopping_Mobile.Services.Implementations;
using Xunit;

namespace Shopping_Mobile.Tests
{
    public class CartServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _cartService = new CartService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task AddToCartAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var request = new OrderItemRequestDTO { ProductId = 1, Quantity = 2 };
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(request.ProductId))
                           .ReturnsAsync((Product?)null);

            // Act & Assert
            await _cartService.Invoking(s => s.AddToCartAsync("user-1", request))
                              .Should()
                              .ThrowAsync<KeyNotFoundException>()
                              .WithMessage("*không tồn tại*"); 
        }

        [Fact]
        public async Task AddToCartAsync_WhenQuantityExceedsStock_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new OrderItemRequestDTO { ProductId = 1, Quantity = 5 };
            var product = new Product { Id = 1, ProductName = "Iphone 15", Stock = 2 };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(request.ProductId))
                           .ReturnsAsync(product);

            // Act & Assert
            await _cartService.Invoking(s => s.AddToCartAsync("user-1", request))
                              .Should()
                              .ThrowAsync<ArgumentException>()
                              .WithMessage("*trong kho*");
        }

        [Fact]
        public async Task AddToCartAsync_WhenItemIsNew_ShouldAddCartItemAndComplete()
        {
            // Arrange
            var request = new OrderItemRequestDTO { ProductId = 1, Quantity = 2 };
            var product = new Product { Id = 1, ProductName = "Iphone 15", Stock = 10 };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(request.ProductId))
                           .ReturnsAsync(product);
            _mockUnitOfWork.Setup(u => u.Carts.GetItemInCartAsync("user-1", request.ProductId))
                           .ReturnsAsync((CartItem?)null);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _cartService.AddToCartAsync("user-1", request);

            // Assert
            _mockUnitOfWork.Verify(u => u.Carts.AddAsync(
                It.Is<CartItem>(item =>
                    item.UserId == "user-1" &&
                    item.ProductId == request.ProductId &&
                    item.Quantity == request.Quantity)), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_WhenItemAlreadyExists_ShouldIncreaseQuantityAndUpdate()
        {
            // Arrange
            var request = new OrderItemRequestDTO { ProductId = 1, Quantity = 2 };
            var product = new Product { Id = 1, ProductName = "Iphone 15", Stock = 10 };
            var existingItem = new CartItem { UserId = "user-1", ProductId = 1, Quantity = 3 };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(request.ProductId))
                           .ReturnsAsync(product);
            _mockUnitOfWork.Setup(u => u.Carts.GetItemInCartAsync("user-1", request.ProductId))
                           .ReturnsAsync(existingItem);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _cartService.AddToCartAsync("user-1", request);

            // Assert
            existingItem.Quantity.Should().Be(5);
            _mockUnitOfWork.Verify(u => u.Carts.Update(existingItem), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateQuantityAsync_WhenNewQuantityIsInvalid_ShouldThrowArgumentException()
        {
            // Act & Assert
            await _cartService.Invoking(s => s.UpdateQuantityAsync("user-1", 1, 0))
                              .Should()
                              .ThrowAsync<ArgumentException>()
                              .WithMessage("*lớn hơn 0*");
        }

        [Fact]
        public async Task UpdateQuantityAsync_WhenCartItemExists_ShouldUpdateQuantityAndComplete()
        {
            // Arrange
            var cartItem = new CartItem { UserId = "user-1", ProductId = 1, Quantity = 1 };
            var product = new Product { Id = 1, Stock = 10 };

            _mockUnitOfWork.Setup(u => u.Carts.GetItemInCartAsync("user-1", 1))
                           .ReturnsAsync(cartItem);
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(1))
                           .ReturnsAsync(product);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _cartService.UpdateQuantityAsync("user-1", 1, 4);

            // Assert
            cartItem.Quantity.Should().Be(4);
            _mockUnitOfWork.Verify(u => u.Carts.Update(cartItem), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveFromCartAsync_WhenItemExists_ShouldRemoveAndComplete()
        {
            // Arrange
            var cartItem = new CartItem { UserId = "user-1", ProductId = 1, Quantity = 1 };
            _mockUnitOfWork.Setup(u => u.Carts.GetItemInCartAsync("user-1", 1))
                           .ReturnsAsync(cartItem);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _cartService.RemoveFromCartAsync("user-1", 1);

            // Assert
            _mockUnitOfWork.Verify(u => u.Carts.Remove(cartItem), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCartByUserIdAsync_WhenUserIdIsEmpty_ShouldThrowUnauthorizedAccessException()
        {
            // Act & Assert
            await _cartService.Invoking(s => s.GetCartByUserIdAsync(string.Empty))
                              .Should()
                              .ThrowAsync<UnauthorizedAccessException>()
                              .WithMessage("*không hợp lệ*");
        }
    }
}