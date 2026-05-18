using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            //  Mapping cho User và Auth
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<RegisterDTO, User>();

            //  Mapping cho Product
            CreateMap<Product, ProductDTO>().ReverseMap();

            CreateMap<OrderRequestDTO, Order>();

            CreateMap<OrderItemRequestDTO, OrderDetail>();

            CreateMap<Order, OrderResponseDTO>();

            // Mapping chi tiết đơn hàng (Lấy thêm thông tin từ bảng Product)
            CreateMap<OrderDetail, ProductDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

            // Mapping cho Cart
            CreateMap<CartItem, ProductDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));
        }
    }
}