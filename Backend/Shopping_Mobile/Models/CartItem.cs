using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Mobile.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public int UserId { get; set; } 
        public decimal PriceAtPurchase { get; set; } 

        // Cấu hình khóa ngoại để EF hiểu cách móc nối dữ liệu
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = default!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = default!;
    }
}