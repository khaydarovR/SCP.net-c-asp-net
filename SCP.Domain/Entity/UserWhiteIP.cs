using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class UserWhiteIP : BaseEntity
    {
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string AllowFrom { get; set; }
    }
}
