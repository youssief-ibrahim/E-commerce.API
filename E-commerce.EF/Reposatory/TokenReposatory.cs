using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace E_commerce.EF.Reposatory
{
    public class TokenReposatory:ITokenReposatory
    {
        private readonly IConfiguration config;
        public TokenReposatory(IConfiguration config)
        {
            this.config = config;
        }
        public async Task<string> GenerateJwtToken(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:SigningKey"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["JWT:ValidIssuerIP"],
                audience: config["JWT:ValidAudienceIP"],
                expires: DateTime.Now.AddHours(1),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public RefreshToken GenerateRefreshToken()
        {
            var random = new byte[32];
            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(random);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(random),
                CreatedOn = DateTime.Now,
                ExpireOn = DateTime.Now.AddDays(10)
            };
        }
        public async Task<bool> RevokeRefreshTokenAsync(string token, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.Now;

            var result = await userManager.UpdateAsync(user);

            return result.Succeeded;
        }
    }
}
