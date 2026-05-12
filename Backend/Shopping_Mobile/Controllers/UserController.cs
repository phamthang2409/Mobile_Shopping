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
        public async Task<ActionResult<UserDTO>> GetProfile(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, UserDTO userDto)
        {
            if (id != userDto.Id) return BadRequest("ID không khớp");

            var result = await _userService.UpdateUserAsync(userDto);
            if (!result) return BadRequest("Cập nhật thất bại");

            return Ok(new { message = "Cập nhật thông tin thành công" });
        }
    }
}