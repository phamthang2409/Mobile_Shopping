using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // 1. Lấy thông tin User và chuyển sang DTO
        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return _mapper.Map<UserDTO>(user);
        }

        // 2. Cập nhật thông tin người dùng
        public async Task<bool> UpdateUserAsync(UserDTO userDto)
        {
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user == null) return false;

            // Ánh xạ dữ liệu mới từ DTO vào User entity hiện tại
            _mapper.Map(userDto, user);

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        // 3. Kiểm tra User tồn tại
        public async Task<bool> UserExistsAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null;
        }
    }
}