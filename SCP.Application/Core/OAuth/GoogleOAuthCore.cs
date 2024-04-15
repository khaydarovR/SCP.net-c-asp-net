using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Core.UserAuth;
using SCP.Application.Core.UserWhiteIP;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Net.Http.Headers;

namespace SCP.Application.Core.OAuth
{
    public class GoogleOAuthCore : BaseCore
    {
        private readonly HttpClient http;
        private readonly ILogger<GoogleOAuthCore> logger;
        private readonly UserAuthCore userAuthCore;
        private readonly JwtService jwtService;
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserWhiteIPCore whiteIPCore;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleOAuthCore(AppDbContext dbContext,
                               HttpClient http,
                               ILogger<GoogleOAuthCore> logger,
                               UserAuthCore userAuthCore,
                               JwtService jwtService,
                               UserManager<AppUser> userManager,
                               IConfiguration configuration,
                               IHttpContextAccessor httpContextAccessor,
                               UserWhiteIPCore whiteIPCore) : base(logger)
        {
            this.http = http;
            this.logger = logger;
            this.userAuthCore = userAuthCore;
            this.jwtService = jwtService;
            this.userManager = userManager;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.whiteIPCore = whiteIPCore;

            _clientId = configuration.GetValue<string>("OAuth:Google:ClientId")!;
            _clientSecret = configuration.GetValue<string>("OAuth:Google:ClientSecret")!;
            logger.LogCritical("SECRETS CONSTRUCTOR INJECT: " + _clientId + " ==== " +_clientSecret);
            this.whiteIPCore = whiteIPCore;
        }

        /// <summary>
        /// Обмен полученного кода на токены Google, создание пользователя и генерация jwt
        /// </summary>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<CoreResponse<AuthResponse>> GetTokens(string code, string scope, string currentIp)
        {
            var request = httpContextAccessor.HttpContext!.Request;
            var host = $"{request.Scheme}://{request.Host}";

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"code", code},
                {"redirect_uri", $"{host}/api/OAuth/Google"},
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"scope", scope},
                {"grant_type", "authorization_code"}
            });

            logger.LogWarning("SCOPE: " + scope);
            logger.LogWarning("CLIENT ID: " + _clientId);
            logger.LogWarning("CLIENT SEC: " + _clientSecret);
            var response = await http.PostAsync("https://oauth2.googleapis.com/token", content);


            if (response.IsSuccessStatusCode == false)
            {
                // You can handle error responses here
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning(errorContent);
                return Bad<AuthResponse>("Token exchange failed: " + response.Content.ReadAsStringAsync().Result.ToString());
            }

            var resultString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GoogleTokenResponseDTO>(resultString);

            var userInfo = await GetUserInfo(result!.AccessToken);

            if (userInfo.IsSuccess == false)
            {
                return Bad<AuthResponse>(userInfo.ErrorList.ToArray());
            }

            var dbUser = await userManager.FindByEmailAsync(userInfo.Data.email);

            if (dbUser == null)
            {
                var createUserResult = await userAuthCore.CreateAccount(new CreateAccountCommand
                {
                    Email = userInfo.Data.email,
                    UserName = userInfo.Data.given_name,
                    FA2Enabled = userInfo.Data.verified_email,
                    Password = null,
                    CurrentIp = currentIp
                });


                if (createUserResult.IsSuccess == false)
                {
                    return Bad<AuthResponse>(createUserResult.ErrorList.ToArray());
                }
            }

            dbUser = await userManager.FindByEmailAsync(userInfo.Data.email);
            await whiteIPCore.Create(dbUser.Id, currentIp);

            var jwt = await jwtService.GenerateJwtToken(dbUser);

            return Good(new AuthResponse
            {
                Jwt = jwt,
                UserId = dbUser.Id.ToString(),
                UserName = dbUser.UserName!,
            });
        }

        public async Task<CoreResponse<GoogleUserInfo>> GetUserInfo(string accessToken)
        {
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await http.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(resultString);
                    return Good(userInfo!);
                }
                else
                {
                    var errorString = await response.Content.ReadAsStringAsync();
                    logger.LogError(errorString);
                    return Bad<GoogleUserInfo>("Error: " + errorString);
                }

            }
        }



    }
}
