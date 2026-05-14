using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using BCrypt.Net;

namespace Shopping_Mobile.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly AppDbContext _context;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ITokenProvider tokenProvider, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
            _context = context;
        }

        public async Task<User?> RegisterAsync(RegisterDTO registerDto)
        {
            // Kiểm tra Username tồn tại
            var existingUserByName = await _unitOfWork.Users.GetByUserNameAsync(registerDto.UserName);
            if (existingUserByName != null) return null;

            // Kiểm tra Email tồn tại
            var existingUserByEmail = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null) return null;

            var user = _mapper.Map<User>(registerDto);
            user.Id = Guid.NewGuid().ToString();

            // --- BCRYPT HASHING ---
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.PassWord);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }

        public async Task<User?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(loginDto.UserName);

            // BCRYPT VERIFY 
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.PassWord, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            // Xóa token cũ để thay token mới 
            var oldTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(oldTokens);

            // Tạo Token mới
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<AuthResponseDTO?> RefreshTokenAsync(RefreshRequestDTO request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

            if (storedToken == null) return null;
            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                _context.RefreshTokens.Remove(storedToken);
                await _unitOfWork.CompleteAsync();
                return null;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
            if (user == null) return null;

            // Tạo bộ Token mới
            var newAccessToken = _tokenProvider.CreateToken(user);
            var newRefreshToken = _tokenProvider.GenerateRefreshToken();

            // Cập nhật Refresh Token hiện tại
            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(7);

            _context.RefreshTokens.Update(storedToken);
            await _unitOfWork.CompleteAsync();

            return new AuthResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDTO>(user)
            };
        }
    }
}