using Microsoft.AspNetCore.Http;
using SCP.Application.Common;
using SCP.Application.Common.Helpers;
using SCP.Application.Core.UserWhiteIP;
using SCP.Application.Services;
using SCP.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SCP.Api.Middleware
{
    public class WhiteIpMiddleware 
    {
        private readonly RequestDelegate _next;

        public WhiteIpMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        string accessDenidedMsg = "Ваш IP аддресс не находится в списке разрешенных IP аддресов, если у вас включен VPN попробуйте выключить его";
        string notFoundIp = "Не удалось получить ваш IP аддресс";

        public async Task Invoke(HttpContext context)
        {
            using var scope = context.RequestServices.CreateScope();
            var whiteIPCore = scope.ServiceProvider.GetRequiredService<UserWhiteIPCore>();
            var rabbitMq = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
            var c = context.User.Claims.ToList();
            var ipAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "User-Agent";
            var jwt = context.Request.Headers["Authorization"].FirstOrDefault() ?? "Jwt";
            ipAddress = ipAddress ?? "0.0.0.1";
            var userId = Helpers.GetId(context.User);
           
            if (ipAddress == null)
            {
                var response = new CoreResponse<bool>(errorText: notFoundIp);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(response.ErrorList);
                return;
            }

            //если пользователь еще не авторизован запрос пропускаем дальше по конвейру
            if (userId == Guid.Empty)
            {
                await _next(context);
                return;
            }

            var controller = context.GetRouteData().Values["controller"]?.ToString() ?? "";
            var action = context.GetRouteData().Values["action"]?.ToString() ?? "";
            var endpointName = $"{controller}/{action}";

            if (endpointName.Contains("User/Info"))
            {
                await _next(context);
                return;
            }
            if (endpointName.Contains("UserWhiteIP/Create"))
            {
                await _next(context);
                return;
            }
            if (endpointName.Contains("/ping"))
            {
                await _next(context);
                return;
            }

            var isAllow = whiteIPCore.IsAllowFrom(ipAddress!.ToString(), userId);
            if (isAllow == false)
            {
                var msgq = new QueueMessage { MsgType = Domain.Enum.MsgType.Question, UserId = userId.ToString() };
                msgq.Jwt = jwt;
                msgq.Payload = 
                    $"Кто то пытался зайти в ваш аккаунта с запрещенного IP адреса: {ipAddress!}\n" +
                    $"Браузер: {userAgent!}\n\n" +
                    $"Добавить этот адресс в список разрешенных?|{ipAddress}" ;
                rabbitMq.SendMessage(msgq);

                var response = new CoreResponse<bool>(errorText: ipAddress + "|" + accessDenidedMsg);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(response.ErrorList);
                return;
            }

            await _next(context);
        }
    }

}
