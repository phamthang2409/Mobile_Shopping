using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByUserNameAsync(string userName);

    }
}