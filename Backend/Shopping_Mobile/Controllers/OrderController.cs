using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using System.Security.Claims;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        // Helper lấy UserId an toàn tuyệt đối từ mã hóa Token của người dùng đang đăng nhập
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lấy lịch sử đơn hàng của User cụ thể
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var orders = await orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        // Lấy tất cả đơn hàng (Dành cho Admin quản lý)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await orderService.GetAllOrderAsync(); 
            return Ok(orders);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] int status)
        {
            await orderService.UpdateOrderStatusAsync(id, status);
            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }

        // Tạo đơn hàng thanh toán (Yêu cầu Token Đăng nhập)
        [Authorize]
        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO request)
        {
            var result = await orderService.CreateOrderAsync(CurrentUserId, request);

            return Ok(new { message = "Đặt hàng thành công!", data = result });
        }
    }
}