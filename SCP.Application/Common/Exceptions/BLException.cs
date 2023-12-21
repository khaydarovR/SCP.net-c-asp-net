using System.Net;

namespace SCP.Application.Common.Exceptions
{
    public class BLException : Exception
    {
        public HttpStatusCode Status { get; init; }
        public BLException(HttpStatusCode status, string message)
            : base(message)
        {
            Status = status;
        }
    }
}
