using System.Security.Claims;
using E_commerce.Core.DTO.Auth;
using E_commerce.Core.IReposatory;
using E_commerce.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Win32;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IStringLocalizer<AccountController> localizer;
        private readonly IEmailService emailService;


        public AccountController(UserManager<ApplicationUser> userManager, IStringLocalizer<AccountController> localizer, RoleManager<ApplicationRole> roleManager, IEmailService emailService)
        {
            this.userManager = userManager;
            this.localizer = localizer;
            this.roleManager = roleManager;
            this.emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await userManager.FindByNameAsync(register.UserName) != null)
                return BadRequest(localizer["username"].Value);

            if (await userManager.FindByEmailAsync(register.Email) != null)
                return BadRequest(localizer["email"].Value);

            ApplicationUser user = new ApplicationUser()
            {
                Name = register.Name,
                UserName = register.UserName,
                Email = register.Email,
            };

            IdentityResult result = await userManager.CreateAsync(user, register.Password);

            if (result.Succeeded)
            {
                #region token email confirmation
                /* with token email confirmation */
                //           await userManager.AddToRoleAsync(user, "User");
                //           //send email confirmation
                //           var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                //           var confirmationLink =
                //$"{Request.Scheme}://{Request.Host}/api/Account/confirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                //           //message service to send the email
                //           await emailService.SendEmailAsync(
                //             //"test@gmail.com",
                //             register.Email,
                //              "Confirm your email",
                //              $"{confirmationLink}"
                //             );
                #endregion

                // with code email confirmation
                Random generator = new Random();
                string code = generator.Next(0, 1000000).ToString("D6");

                user.Code = code;
                user.CodeExpiry = DateTime.Now.AddMinutes(5); // code valid for 5 minutes

                await userManager.UpdateAsync(user);

                
                await emailService.SendEmailAsync(
                   email:  register.Email,
                   subject:  localizer["confirmyouremail"].Value,
                  htmlMessage: $"{localizer["confirmEmailCode"].Value} {code}"
                );
                await userManager.AddToRoleAsync(user, "User");
                return Ok($"{localizer["accountCreatedCheckEmail"].Value} \"{register.Email}\" .");
            }

            foreach (var item in result.Errors)
                ModelState.AddModelError("Password", item.Description);

            return BadRequest(ModelState);
        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string code)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
            if (user.Code != code)
                return BadRequest(localizer["invalidcode"].Value);
            if (user.CodeExpiry < DateTime.Now)
                return BadRequest(localizer["codeexpired"].Value);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                user.Code = null;
                user.CodeExpiry = null;
                await userManager.UpdateAsync(user);
                return Ok(localizer["confirmemail"].Value);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
        [HttpGet("resendcode")]
        public async Task<IActionResult> ResendCode(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
            Random generator = new Random();
            string code = generator.Next(0, 1000000).ToString("D6");
            user.Code = code;
            user.CodeExpiry = DateTime.Now.AddMinutes(5); // code valid for 5 minutes
            await userManager.UpdateAsync(user);
            await emailService.SendEmailAsync(
                     email,
                     localizer["confirmyouremail"].Value,
                     $"{localizer["confirmEmailCode"].Value} {code}"
            );
            return Ok($"{localizer["confirmationCodeResent"].Value} \"{email}\"");
        }
        [HttpPut("ChnageEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null) return Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
            var existingUser = await userManager.FindByEmailAsync(newEmail);
            if (existingUser != null) return BadRequest(localizer["email"].Value);

            Random generator = new Random();
            string code = generator.Next(0, 1000000).ToString("D6");

            user.Code = code;
            user.CodeExpiry = DateTime.Now.AddMinutes(10);
            user.RefrenceNewEmail = newEmail;
            await userManager.UpdateAsync(user);
            await emailService.SendEmailAsync(
                     newEmail,
                     localizer["confirmyouremail"].Value,
                     $"{localizer["confirmEmailCode"].Value} {code}"
             );
            
            return Ok($"{localizer["confirmationCodeResent"].Value} {newEmail}");
        }
        [HttpGet("ConfirmEmailChange")]
        [Authorize]
        public async Task<IActionResult> ConfirmEmailChange(string code)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(localizer["usernotfound"].Value);

            if (user.Code != code)
                return BadRequest(localizer["invalidcode"].Value);

            if (user.CodeExpiry < DateTime.Now)
                return BadRequest(localizer["codeExpired"].Value);

            // Generate the email change token
            var newEmail = user.RefrenceNewEmail;
            var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            var result = await userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
                return BadRequest(localizer["emailChangeFailed"].Value);

            user.Code = null;
            user.CodeExpiry = null;

            await userManager.UpdateAsync(user);

            return Ok(localizer["emailChangeSuccess"].Value);
        }

        [HttpPut("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword( string currentPassword, string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
                return Ok(localizer["passwordChangedSuccessfully"].Value);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
            // Generate code
            Random generator = new Random();
            string code = generator.Next(0, 1000000).ToString("D6");

            user.Code = code;
            user.CodeExpiry = DateTime.Now.AddMinutes(5);

            await userManager.UpdateAsync(user);

            // Send code
            await emailService.SendEmailAsync(
                email,
                localizer["resetPasswordCodeTitle"].Value,
                $"{localizer["resetPasswordCodeMessage"].Value} {code}"
            );

            return Ok($"{localizer["passwordResetCodeSent"].Value} {email}");

        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string mail,string code, string newPassword)
        {
            var user = await userManager.FindByEmailAsync(mail);
            if (user == null) return NotFound(localizer["usernotfound"].Value);
          
            if (user.Code != code)
                return BadRequest("Invalid code");

            if (user.CodeExpiry < DateTime.Now)
                return BadRequest("Code expired");

            
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var result = await userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            user.Code = null;
            user.CodeExpiry = null;

            await userManager.UpdateAsync(user);

            return Ok(localizer["passwordResetSuccess"].Value);
        }
    }
}
