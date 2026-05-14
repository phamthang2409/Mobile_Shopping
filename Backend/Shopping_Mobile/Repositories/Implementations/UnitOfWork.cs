using Shopping_Mobile.Data;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed = false;

        // Các Repository được quản lý bởi Unit of Work
        public IOrderRepository Orders { get; private set; }
        public IProductRepository Products { get; private set; }
        public IUserRepository Users { get; private set; }
        public ICartRepository Carts { get; private set; }      

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Orders = new OrderRepository(_context);
            Products = new ProductRepository(_context);
            Users = new UserRepository(_context);

            Carts = new CartRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}