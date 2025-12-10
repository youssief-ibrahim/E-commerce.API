using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace E_commerce.Core.DTO.OrderC
{
    public class CreateOrderDTO
    {
        [JsonIgnore]
        public DateTime OrderDate { get; set; }
        [JsonIgnore]
        public decimal TotalAmount { get; set; }
        [JsonIgnore]
        public string Status { get; set; } = "Pending";

        // FK
        public int UserId { get; set; }
    }
}
