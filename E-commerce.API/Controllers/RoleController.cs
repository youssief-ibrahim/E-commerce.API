using System.Data;
using E_commerce.Core.Basic;
using System.Security.Claims;
using E_commerce.Core.DTO.Authorize;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles ="Admin")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IStringLocalizer<RoleController> localizer;
        public RoleController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IStringLocalizer<RoleController> localizer)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.localizer = localizer;
        }
        [HttpGet]
        public IActionResult getall()
        {
            var roles = roleManager.Roles.ToList();
            List<RoleDTO>roleDTOs = new List<RoleDTO>();
            foreach (var role in roles)
            {
                roleDTOs.Add(new RoleDTO { Id = role.Id, Name = role.Name });
            }
            return Ok(roleDTOs);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDTO roleDTO)
        {
            if (ModelState.IsValid)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleDTO.Name);
                if (roleExist)
                    return BadRequest(localizer["roleexists"].Value);
                ApplicationRole role = new ApplicationRole
                {
                    Name = roleDTO.Name
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    var admin = await userManager.FindByNameAsync("Admin");
                    if (admin != null)
                    {
                        await userManager.AddToRoleAsync(admin, role.Name);
                    }
                    return Ok(localizer["rolecreated"].Value);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            return BadRequest(ModelState);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDTO roleDTO)
        {
            if (ModelState.IsValid)
            {

                var existingRole = await roleManager.FindByIdAsync(id.ToString());
                if (existingRole == null)
                {
                    return NotFound(localizer["rolenotfound"].Value);
                }
                if (roleDTO.Id != id) return BadRequest("Shoud match id with the curent filed");
                existingRole.Name = roleDTO.Name;
                var result = await roleManager.UpdateAsync(existingRole);
                if (result.Succeeded)
                {
                    return Ok(localizer["roleupdated"].Value);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var existingRole = await roleManager.FindByIdAsync(id); // get role
            if (existingRole == null)
            {
                return NotFound(localizer["rolenotfound"].Value);
            }
            // Check if any user uses this role
            var usersInRole = await userManager.GetUsersInRoleAsync(existingRole.Name!);
            var usersExceptAdmin = usersInRole.Where(u => u.UserName != "Admin"); // case admin get all roles
            if (usersExceptAdmin.Any())
            {
                return BadRequest(localizer["roleinuse"].Value);
            }

            var result = await roleManager.DeleteAsync(existingRole);
            if (result.Succeeded)
            {
                return Ok(localizer["roledeleted"].Value);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("ManageRoles/{userId}")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(localizer["usernotfound"].Value);

            var allRoles = roleManager.Roles.ToList(); // get all roles in the system

            var userRoles = await userManager.GetRolesAsync(user); // get roles for specific user admin for exmple taken all roles

            var roleProps = allRoles.Select(r => new Rolepropertie
            {
                Id = r.Id,
                RoleName = r.Name,
                isselected = userRoles.Contains(r.Name)
            }).ToList();

            var response = new UserRoleClams
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Roleproperties = roleProps
            };

            return Ok(response);
        }


        [HttpPost("ManageRoles/{userId}")]
        public async Task<IActionResult> ManageUserRoles(string userId, [FromBody] UserRoleClamsUpdate dto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(localizer["usernotfound"].Value);
            var userRoles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, userRoles);
            if (!result.Succeeded)
            {
                return BadRequest(localizer["cannotremoveroles"].Value);
            }
            var selectedRoles = dto.Roleproperties.Where(r => r.isselected).Select(r => r.RoleName).ToList();
            result = await userManager.AddToRolesAsync(user, selectedRoles);
            if (!result.Succeeded)
            {
                return BadRequest(localizer["cannotaddroles"].Value);
            }
            return Ok(localizer["rolesupdated"].Value);
        }

        [HttpGet("Permission/{roleId}")]
        public async Task<IActionResult> GetPermissions(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound(localizer["rolenotfound"].Value);

            var roleClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = await Permission.GetAllPermissions(roleManager);

            var response = allPermissions.Select(p => new PermetionProperties
            {
                PermetionName = p,
                isselected = roleClaims.Any(c => c.Type == "Permission" && c.Value == p)
            });

            return Ok(new
            {
                roleId = role.Id,
                roleName = role.Name,
                permissions = response
            });
        }

        [HttpPost("Permission/{roleId}")]
        public async Task<IActionResult> UpdatePermissions(string roleId, [FromBody] List<PermetionProperties> permissions)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound(localizer["rolenotfound"].Value);

            // Remove all old claims
            var currentClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
                await roleManager.RemoveClaimAsync(role, claim);

            // Add only selected permissions
            var selectedPermissions = permissions
                .Where(p => p.isselected)
                .Select(p => p.PermetionName)
                .ToList();

            foreach (var perm in selectedPermissions)
                await roleManager.AddClaimAsync(role, new Claim("Permission", perm));

            return Ok(new
            {
                message = localizer["permissionsupdated"].Value,
                roleId = role.Id,
                roleName = role.Name,
                appliedPermissions = selectedPermissions
            });
        }

    }
}
