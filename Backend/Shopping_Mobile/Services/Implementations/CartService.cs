using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Shopping_Mobile.Services.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class CartService(IUnitOfWork unitOfWork) : ICartService
    {
        public async Task AddToCartAsync(string userId, OrderItemRequestDTO request)
        {
            // Kiểm tra sản phẩm có tồn tại và còn hàng không
            var product = await unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new KeyNotFoundException("Sản phẩm không tồn tại.");

            if (product.Stock < request.Quantity)
                throw new ArgumentException($"Sản phẩm '{product.ProductName}' chỉ còn {product.Stock} trong kho.");

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var existingItem = await unitOfWork.Carts.GetItemInCartAsync(userId, request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;

                // Kiểm tra lại nếu tổng số lượng vượt quá kho sau khi cộng dồn
                if (existingItem.Quantity > product.Stock)
                    throw new ArgumentException("Tổng số lượng trong giỏ hàng vượt quá tồn kho.");

                unitOfWork.Carts.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                await unitOfWork.Carts.AddAsync(newItem);
            }

            var result = await unitOfWork.CompleteAsync();
            if (result <= 0) throw new Exception("Không thể thêm vào giỏ hàng.");
        }

        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Người dùng không hợp lệ.");

            return await unitOfWork.Carts.GetCartByUserIdAsync(userId);
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int newQuantity)
        {
            if (newQuantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");

            var cartItem = await unitOfWork.Carts.GetItemInCartAsync(userId, productId);
            if (cartItem == null) throw new KeyNotFoundException("Không tìm thấy sản phẩm trong giỏ.");

            // Kiểm tra tồn kho trước khi cập nhật số lượng mới
            var product = await unitOfWork.Products.GetByIdAsync(productId);
            if (product != null && newQuantity > product.Stock)
                throw new ArgumentException("Số lượng yêu cầu vượt quá tồn kho.");

            cartItem.Quantity = newQuantity;
            unitOfWork.Carts.Update(cartItem);

            await unitOfWork.CompleteAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            var item = await unitOfWork.Carts.GetItemInCartAsync(userId, productId);
            if (item == null) throw new KeyNotFoundException("Sản phẩm không tồn tại trong giỏ.");

            unitOfWork.Carts.Remove(item);
            await unitOfWork.CompleteAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            unitOfWork.Carts.ClearCart(userId);
            await unitOfWork.CompleteAsync();
        }
    }
}