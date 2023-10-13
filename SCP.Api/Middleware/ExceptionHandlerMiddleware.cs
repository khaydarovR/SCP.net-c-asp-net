using Microsoft.AspNetCore.Http;
using SCP.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace SCP.Application.Common.PipeLine
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (ValidationException exception)
            {
                await HandleValidationsException(ctx, exception);
            }
            catch (BLException exception)
            {
                await HandleBLException(ctx, exception);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(ctx, exception);
            }
        }

        private Task HandleValidationsException(HttpContext ctx, ValidationException exception)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ctx.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error ="Ошибка валидации: " + exception.Message });

            return ctx.Response.WriteAsync(result);
        }

        private Task HandleBLException(HttpContext ctx, BLException exception)
        {
            ctx.Response.StatusCode = (int)exception.Status;
            ctx.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = "Ошибка запроса: " + exception.Message });

            return ctx.Response.WriteAsync(result);
        }

        private Task HandleExceptionAsync(HttpContext ctx, Exception exception)
        {

            var code = HttpStatusCode.InternalServerError;

            ctx.Response.StatusCode = (int)code;
            ctx.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new { error = "Ошибка сервера: " + exception.Message });
   
            return ctx.Response.WriteAsync(result);
        }

    }
}
