using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Net.Http.Headers;

namespace SCP.Application.Core.ApiKey
{
    public class GoogleOAuthCore : BaseCore
    {
        private readonly HttpClient http;
        private readonly ILogger<GoogleOAuthCore> logger;
        private readonly UserAuthCore userAuthCore;
        private readonly JwtService jwtService;
        private readonly UserManager<AppUser> userManager;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleOAuthCore(AppDbContext dbContext,
                               HttpClient http,
                               ILogger<GoogleOAuthCore> logger,
                               UserAuthCore userAuthCore,
                               JwtService jwtService,
                               UserManager<AppUser> userManager,
                               IConfiguration configuration) : base(logger)
        {
            this.http = http;
            this.logger = logger;
            this.userAuthCore = userAuthCore;
            this.jwtService = jwtService;
            this.userManager = userManager;
            _clientId = configuration.GetValue<string>("OAuth:Google:ClientId")!;
            _clientSecret = configuration.GetValue<string>("OAuth:Google:ClientSecret")!;
        }

        /// <summary>
        /// Обмен полученного кода на токены Google, создание пользователя и генерация jwt
        /// </summary>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<CoreResponse<AuthResponse>> GetTokens(string code, string scope)
        {


            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"code", code},
                    {"redirect_uri", "https://localhost:7192/api/OAuth/Google"},
                    {"client_id", this._clientId},
                    {"client_secret", this._clientSecret},
                    {"scope", scope},
                    {"grant_type", "authorization_code"}
                });

            logger.LogWarning(scope);
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
                    Password = null
                });


                if (createUserResult.IsSuccess == false)
                {
                    return Bad<AuthResponse>(createUserResult.ErrorList.ToArray());
                }
            }

            dbUser = await userManager.FindByEmailAsync(userInfo.Data.email);

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
