namespace Shopping_Mobile.DTOs
{
    public class OrderRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<OrderItemRequestDTO> Items { get; set; } = new();
    }

    public class OrderItemRequestDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}