using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // 1. Mapping cho User và Auth
            CreateMap<Models.User, DTOs.UserDTO>().ReverseMap();
            CreateMap<DTOs.RegisterDTO, Models.User>();

            // 2. Mapping cho Product
            CreateMap<Models.Product, DTOs.ProductDTO>().ReverseMap();

            // 3. Mapping cho Order (Nên thêm để dùng cho trang Lịch sử đơn hàng)
            //CreateMap<Order, OrderResponseDTO>().ReverseMap();

            //// Mapping chi tiết đơn hàng (Dịch từ Product sang DTO để lấy tên SP, ảnh...)
            //CreateMap<OrderDetail, OrderDetailResponseDTO>()
            //    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
            //    .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

            // ĐÃ XÓA: Tất cả mapping liên quan đến Cart và CartItem
        }
    }
}