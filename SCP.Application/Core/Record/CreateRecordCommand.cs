using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.Record
{
    public class CreateRecordCommand
    {
    }

    public class ReadRecordCommand
    {
        public string PubKeyFromClient {  get; set; }
        public Guid RecordId {  get; set; }
    }
}
