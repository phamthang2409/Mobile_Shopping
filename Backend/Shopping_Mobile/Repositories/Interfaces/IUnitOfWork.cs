namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IProductRepository Products { get; }
        IUserRepository Users { get; }      

        Task<int> CompleteAsync(); 
    }
}
