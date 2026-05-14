using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Mobile.Models
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}