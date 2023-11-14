using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class BotWhiteIP : BaseEntity
    {
        public Guid BotId { get; set; }
        public Bot Bot { get; set; }
        public string AllowFrom { get; set; }
    }
}
