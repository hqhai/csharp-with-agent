using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.VideoTimeCode
{
    public class CreateVideoTimeCodeCommandModel
    {
        public EnumTimeCodeType TimeCodeType { get; set; }
        public Guid VideoId { get; set; }
        public TimeSpan DisplayTime { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public List<TimeCodeExcercise> TimeCodeExcercises { get; set; } = new List<TimeCodeExcercise>();
    }
}