using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class VideoTimeCode : Entity
    {
        public Int64 DisplayTime { get; set; }

        public Int64 ExecutionTime { get; set; }

        public EnumVideoTimeCode Type { get; set; }

        public Guid VideoId { get; set; }

        [NotMapped]
        public TimeSpan Displaytime
        {
            get { return TimeSpan.FromTicks(DisplayTime); }
            set { DisplayTime = value.Ticks; }
        }

        [NotMapped]
        public TimeSpan DExecutionTime
        {
            get { return TimeSpan.FromTicks(ExecutionTime); }
            set { ExecutionTime = value.Ticks; }
        }
    }
}