using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        Task<IEnumerable<Product>> SearchByNameAsync(string name);

        Task<bool> UpdateStockAsync(int productId, int quantity);

    }
}