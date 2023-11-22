﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Application.Core.Access
{
    public class AuthrizeUsersToSafeCommand
    {
        public Guid AuthorId { get; set; }

        public List<string> SafeIds { get; set; }
        public List<string> UserIds { get; set; }
        public List<string> Permisions { get; set; }
        public int DayLife { get; set; }
    }
}
