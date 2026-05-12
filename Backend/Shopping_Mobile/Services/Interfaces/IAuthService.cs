using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDTO registerDto);
        Task<User?> LoginAsync(LoginDTO loginDto);
        // Hàm xử lý cấp lại token mới
        Task<AuthResponseDTO?> RefreshTokenAsync(RefreshRequestDTO request);
    }
}