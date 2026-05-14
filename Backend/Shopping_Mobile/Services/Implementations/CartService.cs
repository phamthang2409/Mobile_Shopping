using Microsoft.AspNetCore.Cors.Infrastructure;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Shopping_Mobile.Services.Interfaces; 

namespace Shopping_Mobile.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Constructor thực hiện Dependency Injection cho UnitOfWork
        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddToCartAsync(string userId, OrderItemRequestDTO request)
        {
            // 1. Kiểm tra tồn tại trong giỏ hàng thông qua UnitOfWork
            var existingItem = await _unitOfWork.Carts.GetItemInCartAsync(userId, request.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                _unitOfWork.Carts.Update(existingItem);
            }
            else
            {
                // Nếu chưa có, tạo thực thể CartItem mới
                var newItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                await _unitOfWork.Carts.AddAsync(newItem);
            }

            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<CartItem>();

            return await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
        }
     
        public async Task<bool> RemoveFromCartAsync(string userId, int productId)
        {
            var item = await _unitOfWork.Carts.GetItemInCartAsync(userId, productId);
            if (item == null) return false;

            _unitOfWork.Carts.Remove(item);

            return await _unitOfWork.CompleteAsync() > 0;
        }
        public async Task<bool> UpdateQuantityAsync(string userId, int productId, int newQuantity)
        {
            var cartItem = await _unitOfWork.Carts.GetItemInCartAsync(userId, productId);

            if (cartItem == null) return false;

            cartItem.Quantity = newQuantity;

            _unitOfWork.Carts.Update(cartItem);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        public async Task<bool> ClearCartAsync(string userId)
        {
            _unitOfWork.Carts.ClearCart(userId);

            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}