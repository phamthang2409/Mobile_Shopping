using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shopping_Mobile.Data;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Đăng ký người dùng mới
        public async Task<User?> RegisterAsync(RegisterDTO authDto)
        {
            try
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.UserName == authDto.UserName);

                if (userExists) return null;

                var user = _mapper.Map<User>(authDto);
                user.CreatedAt = DateTime.Now;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Register Error]: {ex.Message}");
                throw;
            }
        }

        // Đăng nhập người dùng
        public async Task<User?> LoginAsync(string username, string password)
        {
          
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username && u.PassWord == password);

            return user;
        }
    }
}