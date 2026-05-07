namespace Shopping_Mobile.DTOs
{
    // Đăng ký
    public class RegisterDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string PassWord { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime Dob { get; set; }
    }

    //  Đăng nhập
    public class LoginDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string PassWord { get; set; } = string.Empty;
    }
}