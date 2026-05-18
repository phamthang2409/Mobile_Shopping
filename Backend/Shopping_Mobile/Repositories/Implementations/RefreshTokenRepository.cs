using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Repositories.Implementations
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
        public void RemoveRange(IEnumerable<RefreshToken> entities)
        {
            _context.RefreshTokens.RemoveRange(entities);
        }
    }
}