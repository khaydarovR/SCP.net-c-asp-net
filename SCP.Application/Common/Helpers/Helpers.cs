using System.Security.Claims;

namespace SCP.Application.Common.Helpers
{
    public static class Helpers
    {
        public static Guid GetId(ClaimsPrincipal user)
        {
            var c = user.Claims.ToList();
            var cv = c.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(cv, out var result))
            {
                return result;
            }
            return Guid.Empty;
        }
    }
}
