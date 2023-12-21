using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SCP.Api.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User.Claims.FirstOrDefault();
            if (user == null)
            {
                // not logged in
                context.Result = new JsonResult(new { message = "Вы не авторизованы!" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
