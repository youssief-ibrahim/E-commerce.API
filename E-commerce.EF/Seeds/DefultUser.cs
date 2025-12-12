using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.Basic;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.EF.Seeds
{
    public class DefultUser
    {
        public static async Task GenerateAdmin(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            ApplicationUser user = new ApplicationUser()
            {
                Name = "mr.Admin",
                UserName = "Admin",
                Email = "Admin@gmail.com",
            };
            var userExists = await userManager.FindByNameAsync(user.UserName);
            if (userExists == null)
            {
                await userManager.CreateAsync(user, "Admin@123");
            }
            //  Give the admin ALL roles(always)
            var allRoles = (await roleManager.Roles
           .Select(r => r.Name)
           .ToListAsync());
            userExists = await userManager.FindByNameAsync(user.UserName); // cause i change identity to int

            foreach (var role in allRoles)
            {
                if (!(await userManager.IsInRoleAsync(userExists, role)))
                {
                    await userManager.AddToRoleAsync(userExists, role);
                }
            }
        }
        public  async Task SeedPermissionsAsync( RoleManager<ApplicationRole> roleManager, ApplicationRole role, string module)
        {
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var permissions = Permission.GetPermissionsList(module);

            foreach (var perm in permissions)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == perm))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", perm));
                }
            }
        }
        public static async Task SeedAdminAllPermissions(RoleManager<ApplicationRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null) return;

            var existingClaims = await roleManager.GetClaimsAsync(adminRole);

            var modules = Enum.GetNames(typeof(Moduls));

                foreach (var module in modules)
                {
                    var permissions = Permission.GetPermissionsList(module);

                    foreach (var permission in permissions)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(adminRole, new Claim("Permission", permission));
                        }
                    }
                }
            
        }

    }
}
