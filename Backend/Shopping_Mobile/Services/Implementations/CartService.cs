using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CartService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CartItemResponseDTO>> GetCartByUserIdAsync(int userId)
        {
            var items = await _context.CartItems
                .Include(c => c.Product) 
                .Where(c => c.Cart.UserId == userId)
                .ToListAsync();



            return _mapper.Map<List<CartItemResponseDTO>>(items);
        }

        public async Task<bool> ProcessCheckoutAsync(int userId, List<CartDTO> items)
        {
            // Tìm hoặc tạo giỏ hàng gốc cho User 
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Lưu danh sách sản phẩm từ FE vào bảng CartItems
            foreach (var item in items)
            {
                var orderItem = new CartItem
                {
                    CartId = cart.Id,
                    UserId = userId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Price 
                };
                _context.CartItems.Add(orderItem);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Order>> GetAllOrderAsync() {
                return await _context.Orders.ToListAsync();
                } 
        public async Task<bool> CreateOrderAsync(int userId, OrderRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newOrder = new Order
                {
                    UserId = userId.ToString(),
                    OrderDate = DateTime.Now,
                    TotalAmount = request.Items.Sum(x => x.Price * x.Quantity),
                    Name = request.Name,
                    Address = request.Address,
                    Phone = request.Phone,
                    Note = request.Note,
                    Status = 0 
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); 

                //  Lưu chi tiết từng sản phẩm vào OrderDetail
                foreach (var item in request.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    _context.OrderDetails.Add(detail);
                }

                //  Xóa sản phẩm trong giỏ hàng sau khi đặt thành công
                var userCart = _context.CartItems.Where(c => c.Cart.UserId == userId);
                _context.CartItems.RemoveRange(userCart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}