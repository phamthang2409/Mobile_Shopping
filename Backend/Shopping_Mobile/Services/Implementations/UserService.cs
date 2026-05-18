using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Repositories.Interfaces;

namespace Shopping_Mobile.Services.Implementations
{
    // Sử dụng Primary Constructor cực gọn, đúng chuẩn C# 12
    public class UserService(IUnitOfWork unitOfWork, IMapper mapper) : IUserService
    {
        public async Task<UserDTO> GetUserByIdAsync(string id)
        {
            var user = await unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {id}");

            return mapper.Map<UserDTO>(user);
        }

        // Hàm chỉ nhận gói DTO gọn gàng, vì Controller đã check trùng ID hộ rồi
        public async Task UpdateUserAsync(UserDTO userDto)
        {
            // 1. Kiểm tra tài khoản có thực sự tồn tại trong DB không
            var existingUser = await unitOfWork.Users.GetByIdAsync(userDto.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("Tài khoản không tồn tại trên hệ thống để cập nhật.");

            // 2. Ánh xạ đè các trường thông tin thay đổi từ DTO vào Entity cũ
            mapper.Map(userDto, existingUser);

            // 3. Đánh dấu cập nhật vào Repository
            unitOfWork.Users.Update(existingUser);

            // 4. Lưu thay đổi xuống cơ sở dữ liệu
            var result = await unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("Lỗi hệ thống: Không thể cập nhật thông tin tài khoản.");
        }
    }
}