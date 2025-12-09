using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.DTO.CartItem;

namespace E_commerce.Core.DTO.ShoppingCart
{
    public class AllShoppingCartDTO
    {
        public int Id { get; set; }

        //FK
        public int UserId { get; set; }
        public List<AllCartItemDTO> Items { get; set; } = new();
    }
}
