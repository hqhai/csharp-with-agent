using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Commands.Excercise;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.VideoTimeCode
{
    public class UpdateVideoTimeCodeCommandModel : BaseCommandModel
    {
        public long DisplayTimeTicks { get; set; }
        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }
        public TimeSpan DisplayTime { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public List<UpdateExcerciseCommandModel> Excercises { get; set; } = new List<UpdateExcerciseCommandModel>();
    }
}