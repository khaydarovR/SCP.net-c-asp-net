using SCP.Application.Common;
using SCP.Application.Core.UserWhiteIP;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SCP.Api.Middleware
{
    public class WhiteIPMiddleware
    {
        private readonly RequestDelegate _next;

        public WhiteIPMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        string accessDenidedMsg = "Ваш IP аддресс не находится в списке разрешенных IP аддресов, если у вас включен VPN попробуйте выключить его";
        string notFoundIp = "Не удалось получить ваш IP аддресс";

        public async Task Invoke(HttpContext context)
        {
            using var scope = context.RequestServices.CreateScope();
            var whiteIPCore = scope.ServiceProvider.GetRequiredService<UserWhiteIPCore>();

            var ipAddress = context.Connection.RemoteIpAddress.MapToIPv4();
            var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.NameId);

            if (ipAddress is null)
            {
                var response = new CoreResponse<bool>(errorText: notFoundIp);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(response.ErrorList);
                return;
            }

            //если пользователь еще не авторизован запрос пропускаем дальше по конвейру
            if (userId == null)
            {
                await _next(context);
                return;
            }

            var isAllow = whiteIPCore.IsAllowFrom(ipAddress!.ToString(), Guid.Parse(userId));

            if (isAllow == false)
            {
                var response = new CoreResponse<bool>(errorText: accessDenidedMsg);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(response.ErrorList);
                return;
            }

            await _next(context);
        }
    }

}
