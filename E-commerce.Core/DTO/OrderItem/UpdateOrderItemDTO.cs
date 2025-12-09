using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Core.DTO.OrderItem
{
    public class UpdateOrderItemDTO
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
