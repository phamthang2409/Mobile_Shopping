using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task AddProductAsync(ProductDTO productDto);
        Task UpdateProductAsync(int id, ProductDTO productDto);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<ProductDTO>> SearchProductsAsync(string name);
    }
}