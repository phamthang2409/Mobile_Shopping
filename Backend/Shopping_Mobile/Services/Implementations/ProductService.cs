using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces; // Hoặc Shopping_Mobile.Services.Interfaces tùy bạn đặt
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class ProductService : IProductService
    {
        // Thay thế DbContext bằng Repository
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            // Logic lấy dữ liệu giờ giao phó cho Repository
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> AddProductAsync(ProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            product.CreatedAt = DateTime.Now;

            // Gọi hàm Add từ Repo (Nhớ bổ sung hàm AddAsync vào Repo nếu chưa có)
            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            return product;
        }

        public async Task<bool> RemoveFromProduct(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return false;

            // Nếu trong IProductRepository chưa có hàm Delete, 
            // Sơn có thể bổ sung hoặc dùng tạm cơ chế xử lý trực tiếp qua Repo
            await _productRepository.DeleteAsync(product);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> SearchProductsByNameAsync(string name)
        {
            // Repo nên lo cả việc filter/search để Service luôn gọn gàng
            return await _productRepository.SearchByNameAsync(name);
        }
    }
}