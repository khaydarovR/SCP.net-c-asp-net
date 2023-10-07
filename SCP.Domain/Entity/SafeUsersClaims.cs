using SCP.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class SafeUsersClaims
    {
        public Guid SafeUsersId { get; set; }
        public ClaimValuesEnum ClaimValue { get; set; }
    }
}
