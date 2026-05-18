using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Interfaces
{
   
    public interface IUserService
    {
        Task<UserDTO> GetUserByIdAsync(string id);
        Task UpdateUserAsync(UserDTO userDto);
    }
}