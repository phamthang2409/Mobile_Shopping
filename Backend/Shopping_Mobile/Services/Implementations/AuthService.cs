using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;

namespace Shopping_Mobile.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly AppDbContext _context;

        public AuthService(IUserRepository userRepository, IMapper mapper, ITokenProvider tokenProvider, AppDbContext context)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tokenProvider = tokenProvider;
            _context = context;
        }

        public async Task<User?> RegisterAsync(RegisterDTO registerDto)
        {
            // 1. Kiểm tra UserName và Email đã tồn tại chưa
            var existingUserByName = await _userRepository.GetByUserNameAsync(registerDto.UserName);
            if (existingUserByName != null) return null;

            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null) return null;

            // Map và gán các giá trị mặc định
            var user = _mapper.Map<User>(registerDto);
            user.Id = Guid.NewGuid().ToString(); 
            user.PasswordHash = registerDto.PassWord; 

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<User?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUserNameAsync(loginDto.UserName);

            // Kiểm tra mật khẩu
            if (user == null || user.PasswordHash != loginDto.PassWord)
                return null;

            return user;
        }

        public async Task<AuthResponseDTO?> RefreshTokenAsync(RefreshRequestDTO request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

            // Kiểm tra nếu token không tồn tại
            if (storedToken == null) return null;

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                _context.RefreshTokens.Remove(storedToken); // Dọn dẹp token hết hạn
                await _context.SaveChangesAsync();
                return null;
            }

            // Tìm User sở hữu Token đó
            var user = await _userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null) return null;

            // Tạo cặp token mới (Access Token mới và Refresh Token mới)
            var newAccessToken = _tokenProvider.CreateToken(user);
            var newRefreshToken = _tokenProvider.GenerateRefreshToken();

            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(7); // Gia hạn thêm 7 ngày từ hiện tại

            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserDTO>(user)
            };
        }
    }
}