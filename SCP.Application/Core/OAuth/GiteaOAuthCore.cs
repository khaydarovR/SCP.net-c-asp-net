using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SCP.Application.Common;
using SCP.Application.Common.Response;
using SCP.Application.Core.ApiKey;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SCP.Application.Core.OAuth
{

    public class GiteaOAuthCore : BaseCore
    {
        private readonly HttpClient http;
        private readonly ILogger<GitHubOAuthCore> logger;
        private readonly UserAuthCore userAuthCore;
        private readonly JwtService jwtService;
        private readonly UserManager<AppUser> userManager;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _host;
        private string _accessToken;

        public GiteaOAuthCore(AppDbContext dbContext,
                               HttpClient http,
                               ILogger<GitHubOAuthCore> logger,
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
            _clientId = configuration.GetValue<string>("OAuth:Gitea:ClientId")!;
            _clientSecret = configuration.GetValue<string>("OAuth:Gitea:ClientSecret")!;
            _host = "https://git.kamaz.tatar/";
        }

        /// <summary>
        /// Обмен полученного кода на АПИ ключ, получение от Gitea, генерация jwt
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<CoreResponse<AuthResponse>> GetTokens(string code)
        {
            var requestContent = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "code", code },
                { "grant_type", "authorization_code"},
                { "redirect_uri", "https://localhost:7192/api/OAuth/Gitea" }
            };

            var response = await SendOAuthTokenRequest(requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning(errorContent);
                return Bad<AuthResponse>("Token exchange failed");
            }

            var result = await ParseOAuthTokenResponse(response);

            _accessToken = result.access_token;
            SStorage.AccessToken = _accessToken;
            var userInfo = await GetUserInfo();

            if (!userInfo.IsSuccess)
            {
                return Bad<AuthResponse>(userInfo.ErrorList.ToArray());
            }

            var dbUser = await GetOrCreateDbUser(userInfo.Data);

            if (dbUser.IsSuccess == false)
            {
                return Bad<AuthResponse>(dbUser.ErrorList.ToArray());
            }

            var jwt = await jwtService.GenerateJwtToken(dbUser.Data);

            return Good(new AuthResponse
            {
                Jwt = jwt,
                UserId = dbUser.Data.Id.ToString(),
                UserName = dbUser.Data.UserName!
            });
        }

        private async Task<HttpResponseMessage> SendOAuthTokenRequest(Dictionary<string, string> requestContent)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _host + "login/oauth/access_token");
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = new FormUrlEncodedContent(requestContent);

            return await http.SendAsync(requestMessage);
        }

        private async Task<GiteaOAuthResponseDTO> ParseOAuthTokenResponse(HttpResponseMessage response)
        {
            var resultString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GiteaOAuthResponseDTO>(resultString);
        }


        private async Task<CoreResponse<AppUser>> GetOrCreateDbUser(GiteaUserInfo userInfo)
        {
            var dbUser = await userManager.FindByEmailAsync(userInfo.email);

            if (dbUser == null)
            {
                var createUserResult = await userAuthCore.CreateAccount(new CreateAccountCommand
                {
                    Email = userInfo.email,
                    UserName = userInfo.login,
                    FA2Enabled = true,
                    Password = null
                });

                if (createUserResult.IsSuccess == false)
                {
                    return Bad<AppUser>(createUserResult.ErrorList.ToArray());
                }
            }
            var res = await userManager.FindByEmailAsync(userInfo.email);

            return Good(res);
        }


        public async Task<CoreResponse<GiteaUserInfo>> GetUserInfo()
        {
            {
                var response = await http.GetAsync(_host + "api/v1/user?access_token=" + _accessToken);

                var email = await GetUserFirstEmail();

                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<GiteaUserInfo>(resultString);
                    userInfo.email = email;
                    logger.LogWarning(resultString);
                    return Good(userInfo);
                }
                else
                {
                    var errorString = await response.Content.ReadAsStringAsync();
                    logger.LogError(errorString);
                    return Bad<GiteaUserInfo>("Error: " + errorString);
                }

            }
        }

        private async Task<string> GetUserFirstEmail()
        {
            var response = await http.GetFromJsonAsync<List<GitHubUserEmailInfo>>(_host + "api/v1/user/emails?access_token=" + _accessToken);
            return response.First(e => e.verified == true).email;
        }
    }
}
