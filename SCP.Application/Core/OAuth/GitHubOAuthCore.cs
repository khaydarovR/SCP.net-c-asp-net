using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SCP.Application.Common.Response;
using SCP.Application.Common;
using SCP.Application.Core.UserAuth;
using SCP.Application.Services;
using SCP.DAL;
using SCP.Domain.Entity;
using Newtonsoft.Json;
using SCP.Application.Core.ApiKey;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SCP.Application.Core.OAuth
{

    public class GitHubOAuthCore : BaseCore
    {
        private readonly HttpClient http;
        private readonly ILogger<GitHubOAuthCore> logger;
        private readonly UserAuthCore userAuthCore;
        private readonly JwtService jwtService;
        private readonly UserManager<AppUser> userManager;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GitHubOAuthCore(AppDbContext dbContext,
                               HttpClient http,
                               ILogger<GitHubOAuthCore> logger,
                               UserAuthCore userAuthCore,
                               JwtService jwtService,
                               UserManager<AppUser> userManager,
                               IConfiguration configuration)
        {
            this.http = http;
            this.logger = logger;
            this.userAuthCore = userAuthCore;
            this.jwtService = jwtService;
            this.userManager = userManager;
            _clientId = configuration.GetValue<string>("OAuth:GitHub:clientId")!;
            _clientSecret = configuration.GetValue<string>("OAuth:GitHub:clientSecret")!;
        }

        /// <summary>
        /// Обмен полученного кода на АПИ ключ, получение от GH userinfo, генерация jwt
        /// </summary>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public async Task<CoreResponse<AuthResponse>> GetTokens(string code, string state)
        {
            var requestContent = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "code", code },
                { "redirect_uri", "https://localhost:7192/api/OAuth/Github" }
            };

            var response = await SendOAuthTokenRequest(requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogWarning(errorContent);
                return Bad<AuthResponse>("Token exchange failed");
            }

            var result = await ParseOAuthTokenResponse(response);

            var userInfo = await GetUserInfo(result.access_token);

            if (!userInfo.IsSuccess)
            {
                return Bad<AuthResponse>(userInfo.ErrorList.ToArray());
            }

            var dbUser = await GetOrCreateDbUser(userInfo.Data);

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
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = new FormUrlEncodedContent(requestContent);

            return await http.SendAsync(requestMessage);
        }

        private async Task<GitHubTokenResponseDTO> ParseOAuthTokenResponse(HttpResponseMessage response)
        {
            var resultString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GitHubTokenResponseDTO>(resultString);
        }


        private async Task<CoreResponse<AppUser>> GetOrCreateDbUser(GitHubUserInfo userInfo)
        {
            var dbUser = await userManager.FindByEmailAsync(userInfo.email);

            if (dbUser == null)
            {
                var createUserResult = await userAuthCore.CreateAccount(new CreateAccountCommand
                {
                    Email = userInfo.email,
                    UserName = userInfo.name,
                    FA2Enabled = userInfo.two_factor_authentication,
                    Password = null
                });

                if (!createUserResult.IsSuccess)
                {
                    return Bad<AppUser>(createUserResult.ErrorList.ToArray());
                }
            }
            var res = await userManager.FindByEmailAsync(userInfo.email);

            return Good(res);
        }


        public async Task<CoreResponse<GitHubUserInfo>> GetUserInfo(string accessToken)
        {
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                http.DefaultRequestHeaders.Add("User-Agent", "Bank Of Secrets"); // Replace "My-CSharp-App" with your own user agent

                var response = await http.GetAsync("https://api.github.com/user");

                var email = await GetUserFirstEmail(http);

                if (response.IsSuccessStatusCode)
                {
                    var resultString = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<GitHubUserInfo>(resultString);
                    userInfo.email = email;
                    logger.LogWarning(resultString);
                    return Good(userInfo);
                }
                else
                {
                    var errorString = await response.Content.ReadAsStringAsync();
                    logger.LogError(errorString);
                    return Bad<GitHubUserInfo>("Error: " + errorString);
                }

            }
        }

        private async Task<string> GetUserFirstEmail(HttpClient http)
        {
            var response = await http.GetFromJsonAsync<List<GitHubUserEmailInfo>>("https://api.github.com/user/emails");
            foreach (var email in response)
            {
                Console.WriteLine($"{email.email} {email.verified} {email.primary}");
            }
            return response.First(e => e.verified == true).email;
        }
    }
}
