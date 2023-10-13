using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class BotRight
    {
        public Guid BotId { get; set; }
        public Bot Bot { get; set; }

        public Guid SafeId { get; set; }
        public Safe Safe { get; set; }

        public string ClaimValue { get; set; }
    }
}
