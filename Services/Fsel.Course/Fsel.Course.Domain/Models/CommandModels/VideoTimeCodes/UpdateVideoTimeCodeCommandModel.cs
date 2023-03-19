using System.Text.Json.Serialization;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Excercises;

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
{
    public class UpdateVideoTimeCodeCommandModel
    {
        public EnumTimeCodeType TimeCodeType { get; set; }
        public long DisplayTime { get; set; }
        public long ExecutionTime { get; set; }
        public List<UpdateExcerciseCommandModel> Excercises { get; set; } = new List<UpdateExcerciseCommandModel>();
    }
}
