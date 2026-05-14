using System.ComponentModel.DataAnnotations;

namespace Shopping_Mobile.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        // Thông tin người nhận
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        public string Name { get; set; } = "Khách hàng";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string Address { get; set; } = string.Empty;

        public int Status { get; set; } = 0;

        public string? Note { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual Bill? Bill { get; set; }
    }
}