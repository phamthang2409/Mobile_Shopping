using AutoMapper;
using FluentAssertions;
using Moq;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Models;
using Shopping_Mobile.Repositories.Interfaces;
using Shopping_Mobile.Services.Implementations;
using Xunit;

namespace Shopping_Mobile.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _userService = new UserService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUserDto()
        {
            // Arrange
            var user = new User { Id = "user-1", UserName = "thang", Email = "thang@example.com" };
            var expectedDto = new UserDTO { Id = "user-1", UserName = "thang", Email = "thang@example.com" };

            _mockUnitOfWork.Setup(u => u.Users.GetByIdAsync("user-1"))
                           .ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDTO>(user))
                       .Returns(expectedDto);

            // Act
            var result = await _userService.GetUserByIdAsync("user-1");

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            _mockUnitOfWork.Verify(u => u.Users.GetByIdAsync("user-1"), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Users.GetByIdAsync("missing-user"))
                           .ReturnsAsync((User?)null);

            // Act & Assert
            await _userService.Invoking(s => s.GetUserByIdAsync("missing-user"))
                              .Should()
                              .ThrowAsync<KeyNotFoundException>()
                              .WithMessage("Không tìm thấy người dùng với ID: missing-user");
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserExists_ShouldMapUpdateAndComplete()
        {
            // Arrange
            var userDto = new UserDTO { Id = "user-1", UserName = "thang", Email = "new@example.com" };
            var existingUser = new User { Id = "user-1", UserName = "thang", Email = "old@example.com" };

            _mockUnitOfWork.Setup(u => u.Users.GetByIdAsync(userDto.Id))
                           .ReturnsAsync(existingUser);
            _mockUnitOfWork.Setup(u => u.CompleteAsync())
                           .ReturnsAsync(1);

            // Act
            await _userService.UpdateUserAsync(userDto);

            // Assert
            _mockMapper.Verify(m => m.Map(userDto, existingUser), Times.Once);
            _mockUnitOfWork.Verify(u => u.Users.Update(existingUser), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userDto = new UserDTO { Id = "missing-user" };
            _mockUnitOfWork.Setup(u => u.Users.GetByIdAsync(userDto.Id))
                           .ReturnsAsync((User?)null);

            // Act & Assert
            await _userService.Invoking(s => s.UpdateUserAsync(userDto))
                              .Should()
                              .ThrowAsync<KeyNotFoundException>()
                              .WithMessage("Tài khoản không tồn tại trên hệ thống để cập nhật.");
        }
    }
}
