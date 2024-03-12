using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SCP.Api.Controllers.Base;
using SCP.Application.Common.Response;
using SCP.Application.Core.OAuth;
using SCP.Application.Services;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : CustomController
    {
        private readonly GoogleOAuthCore googleOauthCore;
        private readonly GitHubOAuthCore gitHubOAuthCore;
        private readonly GiteaOAuthCore giteaOAuthCore;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly EmailService email;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oauthCore"></param>
        /// <param name="gitHubOAuthCore"></param>
        /// <param name="twoFactorAuthService"></param>
        /// <param name="email"></param>
        public OAuthController(GoogleOAuthCore oauthCore, GitHubOAuthCore gitHubOAuthCore, TwoFactorAuthService twoFactorAuthService, EmailService email, GiteaOAuthCore giteaOAuthCore)
        {
            this.googleOauthCore = oauthCore;
            this.gitHubOAuthCore = gitHubOAuthCore;
            this.twoFactorAuthService = twoFactorAuthService;
            this.email = email;
            this.giteaOAuthCore = giteaOAuthCore;
        }

        /// <summary>
        /// Получает код из ответа гугла (запрос в гугл отправлял клиент) и отправляет запрос что бы поменять его на токены
        /// </summary>
        [HttpGet("Google")]
        public async Task<ActionResult<AuthResponse>> GoogleGetCode([FromQuery] string code, string scope, string? state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Missing code");
            }
            var res = await googleOauthCore.GetTokens(code, scope, CurrentIp);

            return res.IsSuccess ? Redirect($"{state}{res.Data.Jwt}") : BadRequest(res.ErrorList);
        }


        [HttpGet("Github")]
        public async Task<ActionResult<AuthResponse>> GitHubGetCode([FromQuery] string code, string state)
        {
            var res = await gitHubOAuthCore.GetTokens(code);
            return res.IsSuccess ? Redirect($"{state}{res.Data.Jwt}") : BadRequest(res.ErrorList);
        }

        [HttpGet("Gitea")]
        public async Task<ActionResult<AuthResponse>> GiteaGetCode([FromQuery] string code, string state)
        {
            var res = await giteaOAuthCore.GetTokens(code);
            return res.IsSuccess ? Redirect($"{state}{res.Data.Jwt}") : BadRequest(res.ErrorList);
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
