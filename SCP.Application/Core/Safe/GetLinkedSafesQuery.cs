using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.Safe
{
    public class GetLinkedSafesQuery
    {
        public GetLinkedSafesQuery(Guid id)
        {
            UserId = id;
        }
        public Guid UserId { get; private set; }
    }
}
