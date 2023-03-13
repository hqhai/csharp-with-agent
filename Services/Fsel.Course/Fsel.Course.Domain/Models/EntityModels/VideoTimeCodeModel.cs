using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeModel : BaseEntityModel
    {
        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        public Guid VideoId { get; set; }

        public TimeSpan DisplayTime { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public List<ExcerciseModel>? Excercises { get; set; }
    }
}
