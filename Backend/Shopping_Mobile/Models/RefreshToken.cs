namespace Shopping_Mobile.Models
{
    public class RefreshToken
    {
        public string Id { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        public User User { get; set; }
    }
} 