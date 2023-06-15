using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Entities;

namespace Fsel.System.Domain.Entities
{
    public class LiveTimeFrame : Entity
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
