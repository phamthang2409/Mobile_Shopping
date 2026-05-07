namespace Shopping_Mobile.Models
{
        public class OrderDetail
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }

            // Liên kết với bảng Order
            public virtual Order Order { get; set; }
            public virtual Product Product { get; set; }
        }
}
