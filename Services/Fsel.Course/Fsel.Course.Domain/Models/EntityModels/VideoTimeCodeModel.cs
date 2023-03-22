using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeModel : BaseModel
    {
        public long DisplayTimeTicks { get; set; }

        public long ExecutionTimeTicks { get; set; }

        public EnumTimeCodeType TimeCodeType { get; set; }

        public Guid VideoId { get; set; }

        public long DisplayTime { get; set; }

        public long ExecutionTime { get; set; }

        public ICollection<ExerciseModel>? Exercises { get; set; }
    }
}
