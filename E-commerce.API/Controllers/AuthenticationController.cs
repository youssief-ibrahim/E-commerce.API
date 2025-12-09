using E_commerce.Core.DTO.Auth;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ITokenReposatory tokenService;
        private readonly IStringLocalizer<AuthenticationController> localizer;

        public AuthenticationController(UserManager<ApplicationUser> userManager, ITokenReposatory tokenService, IStringLocalizer<AuthenticationController> localizer)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            this.localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);

            if (user == null)
                return BadRequest(localizer["invalidusername"].Value);

            if (!await userManager.CheckPasswordAsync(user, dto.Password))
                return BadRequest(localizer["invalidpassword"].Value);
            if (!user.EmailConfirmed)
                return BadRequest(localizer["emailnotconfirmed"].Value);

            // Generate access token
            var accessToken = await tokenService.GenerateJwtToken(user, userManager);

            var newRefreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);

            SetRefreshTokenInCookie(newRefreshToken.Token, newRefreshToken.ExpireOn);

            return Ok(new TokenResnoseDTO
            {
                Token = accessToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpires = newRefreshToken.ExpireOn,
                IsAuthanticated = true
            });
        }
        // remember me 
        [HttpGet("Remember_me")]
        public async Task<IActionResult> RememberMe()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(localizer["tokenisrequired"].Value);
            var user = await userManager.Users
                .Include(x => x.RefreshTokens)
                .SingleOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));
            if (user == null)
                return BadRequest(localizer["invalidrefreshtoken"].Value);
            var storedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
            if (storedToken == null || !storedToken.IsActive)
                return BadRequest(localizer["invalidorrevokedrefreshtoken"].Value);
            var accessToken = await tokenService.GenerateJwtToken(user, userManager);
            return Ok(new TokenResnoseDTO
            {
                Token = accessToken,
                RefreshToken = storedToken.Token,
                RefreshTokenExpires = storedToken.ExpireOn,
                IsAuthanticated = true
            });
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(localizer["tokenisrequired"].Value);
            var result = await tokenService.RevokeRefreshTokenAsync(refreshToken, userManager);
            if (!result)
                return BadRequest(localizer["tokeninvalidoralreadyrevoked"].Value);
            Response.Cookies.Delete("refreshToken");
            return Ok(localizer["loggedoutsuccessfully"].Value);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var user = await userManager.Users
                .Include(x => x.RefreshTokens)
                .SingleOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user == null)
                return BadRequest(localizer["invalidrefreshtoken"].Value);

            var storedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive)
                return BadRequest(localizer["invalidorrevokedrefreshtoken"].Value);

            // 🔥 If NOT EXPIRED → use it (DO NOT create new)
            if (storedToken.ExpireOn > DateTime.Now)
            {
                var newAccessToken = await tokenService.GenerateJwtToken(user, userManager);

                return Ok(new TokenResnoseDTO
                {
                    Token = newAccessToken,
                    RefreshToken = storedToken.Token,
                    RefreshTokenExpires = storedToken.ExpireOn,
                    IsAuthanticated = true
                });
            }

            // ❗ If EXPIRED → create a new refresh token
            var newRefresh = tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(newRefresh);
            await userManager.UpdateAsync(user);

            var accessToken2 = await tokenService.GenerateJwtToken(user, userManager);

            return Ok(new TokenResnoseDTO
            {
                Token = accessToken2,
                RefreshToken = newRefresh.Token,
                RefreshTokenExpires = newRefresh.ExpireOn,
                IsAuthanticated = true
            });
        }


        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(localizer["tokenisrequired"].Value);

            var result = await tokenService.RevokeRefreshTokenAsync(token, userManager);

            if (!result)
                return BadRequest(localizer["tokeninvalidoralreadyrevoked"].Value);

            return Ok(localizer["tokenrevoked"].Value);
        }

        private void SetRefreshTokenInCookie(string token, DateTime expire)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = expire
            };

            Response.Cookies.Append("refreshToken", token, options);
        }
    }
}
