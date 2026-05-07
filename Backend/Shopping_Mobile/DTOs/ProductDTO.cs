namespace Shopping_Mobile.DTOs
{
    public class ProductDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
    }
}