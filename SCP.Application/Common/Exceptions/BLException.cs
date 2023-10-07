using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
