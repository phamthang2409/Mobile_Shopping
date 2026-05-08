using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly TokenProvider _tokenProvider;

        public AuthController(IAuthService authService, AppDbContext context, TokenProvider tokenProvider)
        {
            _authService = authService;
            _context = context;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authService.RegisterAsync(registerDto);

            if (user == null)
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });
            }

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _authService.LoginAsync(
                loginDto.UserName,
                loginDto.PassWord
            );

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Sai tài khoản hoặc mật khẩu!"
                });
            }

            var accessToken = _tokenProvider.Create(user);

            var refreshToken = _tokenProvider.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken,

                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Role
                }
            });
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.Token == request.RefreshToken &&
                    !x.IsRevoked);

            if (storedToken == null)
            {
                return Unauthorized(new
                {
                    message = "Refresh token không hợp lệ"
                });
            }

            if (storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return Unauthorized(new
                {
                    message = "Refresh token đã hết hạn"
                });
            }

            var user = await _context.Users
                .FindAsync(storedToken.UserId);

            if (user == null)
            {
                return Unauthorized();
            }

            // Access token mới
            var newAccessToken = _tokenProvider.Create(user);

            // Refresh token mới
            var newRefreshToken = _tokenProvider.GenerateRefreshToken();

            // Update token cũ
            storedToken.Token = newRefreshToken;
            storedToken.ExpiryDate = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
    }
}