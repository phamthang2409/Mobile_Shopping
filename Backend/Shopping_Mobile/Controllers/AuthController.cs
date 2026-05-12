using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using AutoMapper;
using Shopping_Mobile.Models;
using Shopping_Mobile.Data; 

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IMapper mapper, AppDbContext context)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _mapper = mapper;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _authService.RegisterAsync(registerDto);
            if (user == null) return BadRequest(new { message = "Tên đăng nhập hoặc Email đã tồn tại!" });
            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _authService.LoginAsync(loginDto);
            if (user == null) return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu!" });

            //  Tạo cặp Token
            var accessToken = _tokenProvider.CreateToken(user);
            var refreshTokenValue = _tokenProvider.GenerateRefreshToken();

            //Lưu Refresh Token vào Database
            var oldTokens = _context.RefreshTokens.Where(t => t.UserId == user.Id);
            _context.RefreshTokens.RemoveRange(oldTokens);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7), 
                IsRevoked = false,              
            };

            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            //  Trả về dữ liệu
            return Ok(new
            {
                accessToken,
                refreshToken = refreshTokenValue,
                user = _mapper.Map<UserDTO>(user)
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { message = "Refresh Token không được để trống" });

            var result = await _authService.RefreshTokenAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn" });
            }

            return Ok(result);
        }
    }
}