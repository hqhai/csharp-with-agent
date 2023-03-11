using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Entities
{
    public class VideoTimeCodeModel : BaseEntityModel
    {
        public TimeSpan DisplayTime { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }
        public Guid VideoId { get; set; }

        public List<ExerciseModel>? Excercises { get; set; }
    }
}