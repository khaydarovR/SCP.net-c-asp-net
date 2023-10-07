using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.UserAuth.Queries
{
    public class GetJWTQuery : IRequest<string>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
