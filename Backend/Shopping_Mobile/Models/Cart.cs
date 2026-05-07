using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Mobile.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // Quan hệ 1-n: Một giỏ hàng có nhiều món hàng
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        internal bool Any()
        {
            throw new NotImplementedException();
        }
    }
}