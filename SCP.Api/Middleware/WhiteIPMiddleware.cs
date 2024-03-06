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
            ipAddress = ipAddress ?? "127.321.3213.1";
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

            var controllerName = context.GetRouteData().Values["controller"].ToString();
            if (controllerName.Contains("User"))
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
                    $"Браузер: {userAgent!}\n" +
                    $"Добавить этот адресс в список разрешенных?|{ipAddress}" ;
                rabbitMq.SendMessage(msgq);

                var response = new CoreResponse<bool>(errorText: accessDenidedMsg);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(response.ErrorList);
                return;
            }

            await _next(context);
        }
    }

}
