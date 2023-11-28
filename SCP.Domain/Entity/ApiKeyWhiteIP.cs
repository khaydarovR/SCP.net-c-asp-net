using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class ApiKeyWhiteIP : BaseEntity
    {
        public Guid ApiKeyId { get; set; }
        public ApiKey ApiKey { get; set; }
        public string AllowFrom { get; set; }
    }
}
