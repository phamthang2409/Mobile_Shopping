using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Interfaces
{
    public interface IAuthService
    {
  
        Task RegisterAsync(RegisterDTO registerDto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RefreshTokenAsync(RefreshRequestDTO request);
        Task SaveRefreshTokenAsync(string userId, string refreshTokenValue);
    }
}