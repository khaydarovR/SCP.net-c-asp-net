using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.UserAuth
{
    public class GetJwtQuery
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string Fac { get; set; }

    }
}
