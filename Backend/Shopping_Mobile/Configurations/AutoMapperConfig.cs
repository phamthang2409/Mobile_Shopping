using AutoMapper;
using Shopping_Mobile.DTOs;
using Shopping_Mobile.Models;

namespace Shopping_Mobile.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // Tạo mapping giữa các lớp Model và DTO
            CreateMap<Models.User, DTOs.UserDTO>().ReverseMap();
            CreateMap<Models.Product, DTOs.ProductDTO>().ReverseMap();
            CreateMap<Models.Cart, DTOs.CartDTO>().ReverseMap();
            CreateMap<DTOs.RegisterDTO, Models.User>();
            CreateMap<CartDTO, CartItem>();

           
            CreateMap<CartItem, CartItemResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl));

            CreateMap<Cart, CartDTO>().ReverseMap();

        }
    }
}
