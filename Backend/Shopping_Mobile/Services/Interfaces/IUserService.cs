using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
   
    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(string id); 
        Task<bool> UpdateUserAsync(UserDTO userDto);
        Task<bool> UserExistsAsync(string id);
    }
}