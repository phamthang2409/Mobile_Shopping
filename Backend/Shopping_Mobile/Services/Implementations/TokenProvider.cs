using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shopping_Mobile.Models;
using System.Security.Claims;
using System.Text;

public class TokenProvider
{
    private readonly IConfiguration _configuration;

    public TokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Create(User user)
    {
        string secretKey = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret key is not configured.");

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        int expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationInMinutes");

        if (expirationMinutes <= 0)
        {
            expirationMinutes = 10;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),

            SigningCredentials = credentials,

            Issuer = _configuration["Jwt:Issuer"],

            Audience = _configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(tokenDescriptor);
    }
    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
    }
}