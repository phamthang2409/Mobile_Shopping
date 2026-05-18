using Shopping_Mobile.Models;

namespace Shopping_Mobile.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        // Hàm tìm token xem có hợp lệ không để làm mới Access Token
        Task<RefreshToken?> GetByTokenAsync(string token);

        // Hàm lấy toàn bộ token cũ của User để xóa hoặc thu hồi (Revoke) khi Login mới
        Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(string userId);
        void RemoveRange(IEnumerable<RefreshToken> entities);
    }
}