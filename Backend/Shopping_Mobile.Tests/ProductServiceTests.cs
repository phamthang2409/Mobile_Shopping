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
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _productService = new ProductService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProductDTO()
        {
            // Arrange
            int productId = 1;
            var fakeProduct = new Product { Id = productId, ProductName = "Iphone 15", Price = 1000 };

            var expectedDto = new ProductDTO
            {
                ProductName = "Iphone 15",
                Price = 1000,
                Description = "Dế xịn",
                Stock = 10,
                ImageUrl = "iphone15.jpg"
            };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync(fakeProduct);

            _mockMapper.Setup(m => m.Map<ProductDTO>(fakeProduct))
                       .Returns(expectedDto);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.ProductName.Should().Be("Iphone 15");
            result.Price.Should().Be(1000);

            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_WhenProductsExist_ShouldReturnProductDTOs()
        {
            // Arrange
            var products = new List<Product>
            {
                new() { Id = 1, ProductName = "Iphone 15", Price = 1000 },
                new() { Id = 2, ProductName = "Samsung S24", Price = 900 }
            };

            var expectedDtos = new List<ProductDTO>
            {
                new() { ProductName = "Iphone 15", Price = 1000 },
                new() { ProductName = "Samsung S24", Price = 900 }
            };

            _mockUnitOfWork.Setup(u => u.Products.GetAllAsync())
                           .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(products))
                       .Returns(expectedDtos);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockUnitOfWork.Verify(u => u.Products.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int nonExistentId = 99;

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(nonExistentId))
                           .ReturnsAsync((Product?)null);

            // Act & Assert
            await _productService.Invoking(s => s.GetProductByIdAsync(nonExistentId))
                                 .Should()
                                 .ThrowAsync<KeyNotFoundException>()
                                 .WithMessage($"Không tìm thấy sản phẩm với ID {nonExistentId}");
        }

        [Fact]
        public async Task AddProductAsync_WhenSaveSucceeds_ShouldAddProductAndComplete()
        {
            // Arrange
            var productDto = new ProductDTO
            {
                ProductName = "Iphone 15",
                Price = 1000,
                Stock = 10
            };
            var fakeProduct = new Product { ProductName = "Iphone 15", Price = 1000 };

            _mockMapper.Setup(m => m.Map<Product>(productDto))
                       .Returns(fakeProduct);

            _mockUnitOfWork.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
                           .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _productService.AddProductAsync(productDto);

            // Assert
            _mockUnitOfWork.Verify(u => u.Products.AddAsync(fakeProduct), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_WhenSaveFails_ShouldThrowException()
        {
            // Arrange
            var productDto = new ProductDTO { ProductName = "Iphone 15" };

            _mockMapper.Setup(m => m.Map<Product>(productDto))
                       .Returns(new Product());

            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(0);

            // Act & Assert
            await _productService.Invoking(s => s.AddProductAsync(productDto))
                                 .Should()
                                 .ThrowAsync<Exception>();
                                 
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductExists_ShouldMapUpdateAndComplete()
        {
            // Arrange
            const int productId = 1;
            var productDto = new ProductDTO { ProductName = "Iphone 15 Pro", Price = 1200 };
            var existingProduct = new Product { Id = productId, ProductName = "Iphone 15", Price = 1000 };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync(existingProduct);

            _mockMapper.Setup(m => m.Map(productDto, existingProduct))
                       .Returns(existingProduct);

            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _productService.UpdateProductAsync(productId, productDto);

            // Assert
            _mockMapper.Verify(m => m.Map(productDto, existingProduct), Times.Once);
            _mockUnitOfWork.Verify(u => u.Products.Update(existingProduct), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            const int productId = 99;
            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync((Product?)null);

            // Act & Assert
            await _productService.Invoking(s => s.UpdateProductAsync(productId, new ProductDTO()))
                                 .Should()
                                 .ThrowAsync<KeyNotFoundException>()
                                 .WithMessage($"Sản phẩm với ID {productId} không tồn tại để cập nhật.");
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductExists_ShouldRemoveAndComplete()
        {
            // Arrange
            const int productId = 1;
            var product = new Product { Id = productId, ProductName = "Iphone 15" };

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                           .ReturnsAsync(product);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _productService.DeleteProductAsync(productId);

            // Assert
            _mockUnitOfWork.Verify(u => u.Products.Remove(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnMappedProducts()
        {
            // Arrange
            const string keyword = "iphone";
            var products = new List<Product> { new() { Id = 1, ProductName = "Iphone 15" } };
            var expectedDtos = new List<ProductDTO> { new() { ProductName = "Iphone 15" } };

            _mockUnitOfWork.Setup(u => u.Products.SearchByNameAsync(keyword))
                           .ReturnsAsync(products);
            _mockMapper.Setup(m => m.Map<IEnumerable<ProductDTO>>(products))
                       .Returns(expectedDtos);

            // Act
            var result = await _productService.SearchProductsAsync(keyword);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockUnitOfWork.Verify(u => u.Products.SearchByNameAsync(keyword), Times.Once);
        }
    }
}