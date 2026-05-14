using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Lịch sử đơn hàng của user
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                if (orders == null) return Ok(new List<object>());
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var orders = await _orderService.GetAllOrderAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }


        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] int status)
        {
            try
            {
                // Gọi sang Service để thực hiện update vào DB
                var result = await _orderService.UpdateOrderStatusAsync(id, status);

                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng!" });
                }

                return Ok(new { message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> CreateOrder(string userId, [FromBody] OrderRequestDTO request)
        {
            if (request == null || request.Items == null || !request.Items.Any())
                return BadRequest(new { message = "Đơn hàng không có sản phẩm hoặc dữ liệu không hợp lệ!" });

            try
            {
                var result = await _orderService.CreateOrderAsync(userId, request);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}