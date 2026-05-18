using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Sử dụng Primary Constructor giúp inject dependencies gọn gàng
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            await authService.RegisterAsync(registerDto);

            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var result = await authService.LoginAsync(loginDto);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO request)
        {
            var result = await authService.RefreshTokenAsync(request);

            return Ok(result);
        }
    }
}