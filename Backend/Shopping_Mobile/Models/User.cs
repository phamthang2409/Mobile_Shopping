using System.ComponentModel.DataAnnotations;

namespace Shopping_Mobile.Models
{
    public class User
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string PassWord { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime Dob { get; set; }

        public string? Address { get; set; }

        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Role { get; set; } = "Customer"; 
    }
}