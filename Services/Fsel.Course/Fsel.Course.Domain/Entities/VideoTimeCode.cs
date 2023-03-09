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
        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        public Video? Video { get; set; }
        public Guid VideoId { get; set; }

        [NotMapped]
        public TimeSpan DisplayTime
        {
            get { return TimeSpan.FromTicks(DisplayTimeTicks); }
            set { DisplayTimeTicks = value.Ticks; }
        }

        [NotMapped]
        public TimeSpan ExecutionTime
        {
            get { return TimeSpan.FromTicks(ExecutionTimeTicks); }
            set { ExecutionTimeTicks = value.Ticks; }
        }

        public List<TimeCodeExcercise> TimeCodeExcercises { get; set; } = new List<TimeCodeExcercise>();
    }
}