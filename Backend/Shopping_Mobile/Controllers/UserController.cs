using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UserDTO userDto)
        {
            if (id != userDto.Id)
                throw new ArgumentException("ID người dùng không trùng khớp!");

            await userService.UpdateUserAsync(userDto);

            return Ok(new { message = "Cập nhật thông tin thành công." });
        }
    }
}