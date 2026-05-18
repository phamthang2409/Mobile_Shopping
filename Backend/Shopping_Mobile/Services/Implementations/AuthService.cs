using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;

        // FIX: Đã loại bỏ hoàn toàn AppDbContext ra khỏi Constructor
        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ITokenProvider tokenProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
        }

        public async Task RegisterAsync(RegisterDTO registerDto)
        {
            // Kiểm tra tồn tại - Throw lỗi để Middleware bắt
            var existingUser = await _unitOfWork.Users.GetByUserNameAsync(registerDto.UserName);
            if (existingUser != null) throw new ArgumentException("Tên đăng nhập đã tồn tại!");

            var existingEmail = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
            if (existingEmail != null) throw new ArgumentException("Email đã được sử dụng!");

            var user = _mapper.Map<User>(registerDto);
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.PassWord);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(loginDto.UserName);

            // Nếu sai pass hoặc user, throw Unauthorized (401)
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.PassWord, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu!");
            }

            var accessToken = _tokenProvider.CreateToken(user);
            var refreshTokenValue = _tokenProvider.GenerateRefreshToken();

            await SaveRefreshTokenAsync(user.Id, refreshTokenValue);

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                User = _mapper.Map<UserDTO>(user)
            };
        }

        public async Task<AuthResponseDTO> RefreshTokenAsync(RefreshRequestDTO request)
        {
            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked)
                throw new UnauthorizedAccessException("Phiên làm việc không hợp lệ!");

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                _unitOfWork.RefreshTokens.Remove(storedToken);
                await _unitOfWork.CompleteAsync();
                throw new UnauthorizedAccessException("Phiên làm việc đã hết hạn!");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
            if (user == null) throw new KeyNotFoundException("Người dùng không tồn tại!");

            var newAccessToken = _tokenProvider.CreateToken(user);
            var newRefreshToken = _tokenProvider.GenerateRefreshToken();

            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(7);

            _unitOfWork.RefreshTokens.Update(storedToken);
            await _unitOfWork.CompleteAsync();

            return new AuthResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDTO>(user)
            };
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var oldTokens = await _unitOfWork.RefreshTokens.GetTokensByUserIdAsync(userId);

            if (oldTokens != null && oldTokens.Any())
            {
                _unitOfWork.RefreshTokens.RemoveRange(oldTokens);
            }

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();
        }
    }
}