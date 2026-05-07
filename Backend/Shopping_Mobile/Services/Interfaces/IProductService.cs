using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductAsync(ProductDTO productDto);
        Task<bool> RemoveFromProduct(int productId);
        Task<IEnumerable<Product>> SearchProductsByNameAsync(string name);
    }
}