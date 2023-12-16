using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SCP.Api.Controllers.Base;
using SCP.Api.DTO;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : CustomController
    {
        private readonly UserAuthCore userAuthCore;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly EmailService email;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAuthCore"></param>
        /// <param name="twoFactorAuthService"></param>
        /// <param name="email"></param>
        public OAuthController(UserAuthCore userAuthCore, TwoFactorAuthService twoFactorAuthService, EmailService email)
        {
            this.userAuthCore = userAuthCore;
            this.twoFactorAuthService = twoFactorAuthService;
            this.email = email;
        }


        [Authorize]
        [HttpGet("Google")]
        public ActionResult<string> Google()
        {
            return this.User.Identity.Name;
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
            return Redirect("https://www.google.com/accounts/Logout");
        }
    }
}
