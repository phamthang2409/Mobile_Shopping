using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    // Sử dụng Primary Constructor của C# 12 giúp inject dependencies cực gọn
    public class ProductService(IUnitOfWork unitOfWork, IMapper mapper) : IProductService
    {
        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await unitOfWork.Products.GetAllAsync();
            // Map từ List Entity sang List DTO để trả về cho Client
            return mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);

            // Không return null nữa, throw lỗi trực tiếp để Middleware xử lý
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID {id}");

            return mapper.Map<ProductDTO>(product);
        }

        public async Task AddProductAsync(ProductDTO productDto)
        {
            var product = mapper.Map<Product>(productDto);
            product.CreatedAt = DateTime.Now;

            await unitOfWork.Products.AddAsync(product);
            var result = await unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("Lỗi hệ thống: Không thể thêm sản phẩm.");
        }

        public async Task UpdateProductAsync(int id, ProductDTO productDto)
        {
            var existingProduct = await unitOfWork.Products.GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException($"Sản phẩm với ID {id} không tồn tại để cập nhật.");

            // Ánh xạ đè dữ liệu mới từ DTO vào Entity cũ đang được EF theo dõi
            mapper.Map(productDto, existingProduct);

            unitOfWork.Products.Update(existingProduct);
            var result = await unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("Lỗi hệ thống: Không thể cập nhật sản phẩm.");
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Sản phẩm với ID {id} không tồn tại để xóa.");

            // Hàm Remove đồng bộ từ GenericRepository, không cần await
            unitOfWork.Products.Remove(product);

            var result = await unitOfWork.CompleteAsync();
            if (result <= 0)
                throw new Exception("Lỗi hệ thống: Không thể xóa sản phẩm khỏi cơ sở dữ liệu.");
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsAsync(string name)
        {
            var products = await unitOfWork.Products.SearchByNameAsync(name);
            return mapper.Map<IEnumerable<ProductDTO>>(products);
        }
    }
}