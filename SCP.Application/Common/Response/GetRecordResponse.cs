using SCP.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Common.Response
{
    public class GetRecordResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string ForResource { get; set; } = null!;

        public virtual string RightToCurentUser { get; set; } = null!;
    }
}
