using SCP.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain
{
    public class QueueMessage
    {
        public string UserId { get; set; }
        public string Jwt { get; set; }
        public string Payload { get; set; }
        public MsgType MsgType { get; set; }
    }
}
