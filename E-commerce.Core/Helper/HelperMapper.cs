using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using E_commerce.Core.DTO.CartItem;
using E_commerce.Core.DTO.Category;
using E_commerce.Core.DTO.OrderC;
using E_commerce.Core.DTO.OrderItem;
using E_commerce.Core.DTO.Product;
using E_commerce.Core.DTO.ShoppingCart;
using E_commerce.Core.Models;

namespace E_commerce.Core.Helper
{
    public class HelperMapper:Profile
    {
        public HelperMapper()
        {
            //product
            CreateMap<Product, AllProductDTO>().ReverseMap();
            CreateMap<Product, CreateProductDTO>().ReverseMap();
            //category
            CreateMap<Category, AllCategoryDTO>().ReverseMap();
            CreateMap<Category, CreateCategoryDTO>().ReverseMap();
            //order
            CreateMap<Order, AllOrderDTO>().ReverseMap();
            CreateMap<Order, CreateOrderDTO>().ReverseMap();
            //shopping cart
            CreateMap<ShoppingCart, AllShoppingCartDTO>().ForMember(d => d.Items, opt => opt.MapFrom(s => s.CartItems)).ReverseMap();
            CreateMap<ShoppingCart, CreateShoppingCartDTO>().ReverseMap();
            //cart item
            CreateMap<ShoppingCart, AllShoppingCartDTO>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems)).ReverseMap();

            CreateMap<CartItem, AllCartItemDTO>()
             .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                 .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.Price))
                    .ReverseMap();
             
            //order item
            CreateMap<OrderItem, AllOrderItemDTO>().ReverseMap();
            CreateMap<OrderItem, CreateOrderItemDTO>().ReverseMap();

        }
    }
}
