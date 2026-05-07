using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens.Experimental;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _context; 

        public CartController(ICartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                var cartItems = await _cartService.GetCartByUserIdAsync(userId);
                if (cartItems == null) return Ok(new List<CartItemResponseDTO>());
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult > GetAll()
        {
            var orders = await _cartService.GetAllOrderAsync();
            return Ok(orders);
        }

        [Authorize]
        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> CreateOrder(int userId, [FromBody] OrderRequestDTO request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request == null || request.Items == null || !request.Items.Any())
                return BadRequest(new { message = "Đơn hàng không có sản phẩm hoặc dữ liệu không hợp lệ!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;

                // 1. Xác nhận sản phẩm và tính tổng tiền
                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) 
                        return NotFound(new { message = $"Sản phẩm ID {item.ProductId} không tồn tại" });

                    totalAmount += product.Price * item.Quantity;
                }

                // 2. Tạo đối tượng Order mới
                var newOrder = new Order
                {
                    UserId = userId.ToString(), 
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    Name = request.Name,
                    Address = request.Address,
                    Phone = request.Phone,
                    Note = request.Note,
                    Status = 0 
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Lưu để lấy newOrder.Id

                // 3. Lưu chi tiết đơn hàng (OrderDetails)
                foreach (var item in request.Items)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đặt hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}

