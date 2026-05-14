using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Mobile.Models
{
    public class Bill
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "COD";
    }
}