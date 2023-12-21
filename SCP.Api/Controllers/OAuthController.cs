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
using System.Web;
using System;
using System.Security.Cryptography;
using SCP.Application.Core.ApiKey;
using SCP.Application.Common.Response;

namespace SCP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : CustomController
    {
        private readonly GoogleOAuthCore oauthCore;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly EmailService email;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oauthCore"></param>
        /// <param name="twoFactorAuthService"></param>
        /// <param name="email"></param>
        public OAuthController(GoogleOAuthCore oauthCore, TwoFactorAuthService twoFactorAuthService, EmailService email)
        {
            this.oauthCore = oauthCore;
            this.twoFactorAuthService = twoFactorAuthService;
            this.email = email;
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
            var res = await oauthCore.GetTokens(code, scope);

            //редирект на страницу клиента который принимет jwt
            state = state == null ? "http://localhost:4200/register?jwt=" : state;
            return res.IsSuccess ? Redirect($"{state}{res.Data.UserName}") : BadRequest(res.ErrorList);
        }


        [HttpGet("Github")]
        public async Task<ActionResult<AuthResponse>> GitHubGetCode([FromQuery] string code, string scope, string? state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Missing code");
            }
            var res = await oauthCore.GetTokens(code, scope);

            //редирект на страницу клиента который принимет jwt
            state = state == null ? "http://localhost:4200/register?jwt=" : state;
            return res.IsSuccess ? Redirect($"{state}{res.Data.UserName}") : BadRequest(res.ErrorList);
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
