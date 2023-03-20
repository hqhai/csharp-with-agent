using System.Text.Json.Serialization;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Exercises;

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
{
    public class CreateVideoTimeCodeCommandModel
    {
        public EnumTimeCodeType TimeCodeType { get; set; }
        public long DisplayTime { get; set; }
        public long ExecutionTime { get; set; }
        public List<CreateExerciseCommandModel> Exercises { get; set; } = new List<CreateExerciseCommandModel>();
    }
}
