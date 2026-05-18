using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using System.Security.Claims;
using System.Text;

namespace Shopping_Mobile.Services.Implementations
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(User user)
        {
            // Lấy secret key từ appsettings.json
            string secretKey = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret key is not configured.");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            // Đảm bảo lấy đúng giá trị thời hạn token
            int expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationInMinutes");
            if (expirationMinutes <= 0) expirationMinutes = 60; 

            var claims = new Dictionary<string, object>
            {
                { ClaimTypes.NameIdentifier, user.Id }, 
                { ClaimTypes.Name, user.UserName ?? "" },
                { ClaimTypes.Role, user.Role ?? "User" }
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var handler = new JsonWebTokenHandler();

            // Trả về chuỗi JWT hoàn chỉnh
            return handler.CreateToken(tokenDescriptor);
        }

        public string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
        }
    }
}