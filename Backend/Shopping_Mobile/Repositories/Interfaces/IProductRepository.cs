using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task AddAsync(Product product);

        Task DeleteAsync(Product product);

        Task SaveChangesAsync();

        Task<IEnumerable<Product>> SearchByNameAsync(string name);
    }
}