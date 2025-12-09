using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Core.DTO.CartItem
{
    public class CreateCartItemDTO
    {
        public int Quantity { get; set; }
        //FK
        public int ProductId { get; set; }
        public int ShoppingCartId { get; set; }
    }
}
