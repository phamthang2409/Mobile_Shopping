using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _userService.GetUserByIdAsync(id); 
            if (user == null) return NotFound("Không tìm thấy người dùng");

            return Ok(user);
        }
    }
}