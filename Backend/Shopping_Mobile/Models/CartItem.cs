using System.ComponentModel.DataAnnotations;

namespace Shopping_Mobile.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
    }
}