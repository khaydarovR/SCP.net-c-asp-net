using Newtonsoft.Json;
using SCP.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;


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
            var result = JsonConvert.SerializeObject(new CoreResponse<List<string>>("Ошибка валидации: " + exception.Message));

            return ctx.Response.WriteAsync(result);
        }

        private Task HandleBLException(HttpContext ctx, BLException exception)
        {
            ctx.Response.StatusCode = (int)exception.Status;
            ctx.Response.ContentType = "application/json";
            var result = JsonConvert.SerializeObject(new CoreResponse<List<string>>("Ошибка запроса: " + exception.Message));

            return ctx.Response.WriteAsync(result);
        }

        private Task HandleExceptionAsync(HttpContext ctx, Exception exception)
        {

            var code = HttpStatusCode.InternalServerError;

            ctx.Response.StatusCode = (int)code;
            ctx.Response.ContentType = "application/json";

            var msg = new CoreResponse<List<string>>("Ошибка сервера: " + exception.Message);
            var result = JsonConvert.SerializeObject(msg.ErrorList);
   
            return ctx.Response.WriteAsync(result);
        }

    }
}
