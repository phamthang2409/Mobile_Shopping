namespace Shopping_Mobile.DTOs
{
    //  Thêm vào giỏ
    public class CartDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    // Hiển thị giỏ hàng
    public class CartItemResponseDTO
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}