﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP.Domain.Entity
{
    public class BotRight
    {
        public Guid BotId { get; set; }

        public Guid SafeId { get; set; }

        public string Permission { get; set; }
        public DateTime DeadDate { get; set; }

    }
}