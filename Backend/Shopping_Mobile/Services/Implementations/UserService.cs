using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    public class UserService : IUserService
    {
        // Thay thế UserRepository bằng Unit of Work để quản lý tập trung
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDTO?> GetUserByIdAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> UpdateUserAsync(UserDTO userDto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userDto.Id);
            if (user == null) return false;
            _mapper.Map(userDto, user);
            await _unitOfWork.Users.UpdateAsync(user);
            var result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }

        public async Task<bool> UserExistsAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null;
        }
    }
}