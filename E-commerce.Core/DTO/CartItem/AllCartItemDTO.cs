using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Core.DTO.CartItem
{
    public class AllCartItemDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }

    }
}
