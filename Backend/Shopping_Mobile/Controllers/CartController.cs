using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Services.Interfaces;
using System.Security.Claims; // Cần thiết để lấy UserId từ Token

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] OrderItemRequestDTO request)
        {
            // Lấy UserId từ Claims của Token để đảm bảo tính bảo mật
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định danh tính người dùng.");
            }

            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest("Dữ liệu sản phẩm không hợp lệ hoặc thiếu thông tin.");
            }

            try
            {
                // Gọi sang Service để xử lý lưu vào Database qua UnitOfWork
                var result = await _cartService.AddToCartAsync(userId, request);

                if (result)
                {
                    return Ok(new { message = "Sản phẩm đã được lưu vào giỏ hàng hệ thống." });
                }

                return BadRequest("Không thể cập nhật giỏ hàng. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi lưu giỏ hàng: {ex.Message}" });
            }
        }
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] OrderItemRequestDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest("Số lượng phải lớn hơn 0.");
            }

            try
            {
                var result = await _cartService.UpdateQuantityAsync(userId, request.ProductId, request.Quantity);

                if (result) return Ok(new { message = "Cập nhật số lượng thành công." });
                return BadRequest("Không thể cập nhật số lượng.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cartItems = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cartItems);
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            if (result)
                return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng thành công." });

            return BadRequest("Sản phẩm không tồn tại trong giỏ hàng hoặc không thể xóa.");
        }
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _cartService.ClearCartAsync(userId);
            if (result) return Ok(new { message = "Đã dọn sạch giỏ hàng." });

            return BadRequest("Không thể xóa giỏ hàng.");
        }
    }
}