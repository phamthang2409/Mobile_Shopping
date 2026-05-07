using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.DTOs;

namespace Shopping_Mobile.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(e => e.Id == id);
        }
    }
}