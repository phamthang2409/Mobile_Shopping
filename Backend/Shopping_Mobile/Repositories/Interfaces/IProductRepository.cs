using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        // Thêm sản phẩm mới vào hàng chờ 
        Task AddAsync(Product product);
        Task DeleteAsync(Product product);

        Task<IEnumerable<Product>> SearchByNameAsync(string name);

        // Cập nhật số lượng tồn kho khi khách đặt hàng
        Task<bool> UpdateStockAsync(int productId, int quantity);

    }
}