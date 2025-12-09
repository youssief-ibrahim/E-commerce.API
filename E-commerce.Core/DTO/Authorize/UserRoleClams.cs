using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Core.DTO.Authorize
{
    public class UserRoleClams
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<Rolepropertie> Roleproperties { get; set; }
    }
}
