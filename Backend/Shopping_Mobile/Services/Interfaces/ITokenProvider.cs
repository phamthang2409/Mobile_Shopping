using Shopping_Mobile.Models;

namespace Shopping_Mobile.Interfaces
{
    public interface ITokenProvider
    {
        string CreateToken(User user);
        string GenerateRefreshToken(); 
    }
}