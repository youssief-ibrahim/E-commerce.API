using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Core.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? RefrenceNewEmail { get; set; }
        public DateTime? CodeExpiry { get; set; }
        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual ICollection<Order>? Orders { get; set; } = new List<Order>();
        public virtual List<RefreshToken>? RefreshTokens { get; set; }
    }
}
