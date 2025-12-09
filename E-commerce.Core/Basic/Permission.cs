using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Core.Basic
{
    public static class Permission
    {

        public static List<string> GetPermissionsList(string module)
        {
            return new List<string>()
        {
            $"Permission.{module}.View",
            $"Permission.{module}.Create",
            $"Permission.{module}.Edit",
            $"Permission.{module}.Delete"
        };
        }

        public static async Task<List<string>> GetAllPermissions(RoleManager<ApplicationRole> roleManager)
        {
            var allPermissions = new List<string>();
            var modules = await roleManager.Roles.ToListAsync();

            foreach (var module in modules)
            {
                allPermissions.AddRange(GetPermissionsList(module.ToString()));
            }
            return allPermissions;
        }
    }
}
