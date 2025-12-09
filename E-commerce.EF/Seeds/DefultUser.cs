using System;
using System.Collections.Generic;
using System.Linq;
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
                Name= "mr.Admin",
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
            // give the admin ALL Permetion
            var adminClaims = await userManager.GetClaimsAsync(userExists);
            var allPermissions = await Permission.GetAllPermissions(roleManager);

            foreach (var perm in allPermissions)
            {
                if (!adminClaims.Any(c => c.Type == "Permission" && c.Value == perm))
                {
                    await userManager.AddClaimAsync(userExists, new("Permission", perm));
                }
            }
        }

    }
}
