using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;


public interface IAuthService
{
    Task<User?> RegisterAsync(RegisterDTO authDto);
    Task<User?> LoginAsync(string username, string password);
}