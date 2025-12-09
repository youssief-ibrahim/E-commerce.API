using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Core.IReposatory
{
    public interface ITokenReposatory
    {
        Task<string> GenerateJwtToken(ApplicationUser user, UserManager<ApplicationUser> userManager);
        RefreshToken GenerateRefreshToken();
        Task<bool> RevokeRefreshTokenAsync(string token, UserManager<ApplicationUser> userManager);
    }
}
