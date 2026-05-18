using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Services.Interfaces;
using System.Security.Claims;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController(ICartService cartService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Không thể xác định danh tính người dùng.");

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] OrderItemRequestDTO request)
        {
            await cartService.AddToCartAsync(UserId, request);

            return Ok(new { message = "Sản phẩm đã được lưu vào giỏ hàng." });
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] OrderItemRequestDTO request)
        {
            await cartService.UpdateQuantityAsync(UserId, request.ProductId, request.Quantity);

            return Ok(new { message = "Cập nhật số lượng thành công." });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartItems = await cartService.GetCartByUserIdAsync(UserId);
            return Ok(cartItems);
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            await cartService.RemoveFromCartAsync(UserId, productId);
            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await cartService.ClearCartAsync(UserId);
            return Ok(new { message = "Đã dọn sạch giỏ hàng." });
        }
    }
}