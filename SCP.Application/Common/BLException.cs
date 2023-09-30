using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Common
{
    public class BLException: Exception
    {
        public BLException(string name)
            : base($"Ошибка запроса: {name}") { }
    }
}
