using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SCP.Application.Common;
using SCP.Application.Core.Access;
using SCP.Application.Core.ApiKeyC;
using SCP.Application.Core.Safe;
using SCP.Application.Services;
using SCP.Domain.Entity;
using System.Net.Http.Json;
using System.Text;

namespace SCP.Application.Core.GiteaHook
{
    public class GiteaHookCore : BaseCore
    {
        private readonly UserManager<AppUser> userManager;
        private readonly TwoFactorAuthService twoFactorAuthService;
        private readonly SafeCore safeCore;
        private readonly AccessCore accessCore;
        private readonly UserService userService;
        private readonly CacheService cache;
        private readonly JwtService jwt;
        private readonly HttpClient httpClient;

        public GiteaHookCore(UserManager<AppUser> userManager,
                            TwoFactorAuthService twoFactorAuthService,
                            SafeCore safeCore,
                            CacheService cache,
                            AccessCore accessCore,
                            UserService userService,
                            JwtService jwt, ILogger<ApiKeyCore> logger) : base(logger)
        {
            this.userManager = userManager;
            this.twoFactorAuthService = twoFactorAuthService;
            this.safeCore = safeCore;
            this.cache = cache;
            this.accessCore = accessCore;
            this.userService = userService;
            this.jwt = jwt;

            var accessToken = SStorage.AccessToken;
            // Set up the HttpClient to make requests to the Gitea API

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://git.kamaz.tatar/api/v1/");

            // Append the access token as a query parameter to every request
            var uriBuilder = new UriBuilder(httpClient.BaseAddress);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["access_token"] = accessToken;
            uriBuilder.Query = query.ToString();
            httpClient.BaseAddress = uriBuilder.Uri;
        }

        /// <summary>
        /// Create hook for listening all commits and additions in code
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<CoreResponse<bool>> CreateHook(string userId)
        {

            // Get the list of repositories for the user
            var repositoriesResponse = await httpClient.GetAsync($"user/repos");
            _ = repositoriesResponse.EnsureSuccessStatusCode();
            var repositories = await repositoriesResponse.Content.ReadFromJsonAsync<List<Repository>>();

            // Create the webhook for each repository
            foreach (var repository in repositories)
            {
                if (repository.permissions.admin == false)
                {
                    continue;
                }

                var webhookPayload = new
                {
                    type = "gitea",
                    config = new
                    {
                        content_type = "json",
                        url = @"https://localhost:7192/api/GiteaHook/Catch",
                        secret = "secret",
                        insecure_ssl = "0"
                    },
                    events = new[] { "push" }
                };

                var webhookRequestContent = new StringContent(
                    JsonConvert.SerializeObject(webhookPayload),
                    Encoding.UTF8,
                    "application/json");

                var createWebhookResponse = await httpClient.PostAsync($"repos/{repository.owner.username}/{repository.name}/hooks", webhookRequestContent);
                _ = createWebhookResponse.EnsureSuccessStatusCode();
            }

            return Good(true);
        }




    }
}
