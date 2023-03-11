using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Common.Models.Commands.Excercise;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.VideoTimeCode
{
    public class UpdateVideoTimeCodeCommandModel
    {
        public Guid Id { get; set; }
        public EnumTimeCodeType TimeCodeType { get; set; }
        public string? DisplayTimeStr { get; set; }
        public string? ExecutionTimeStr { get; set; }

        [JsonIgnore]
        public TimeSpan DisplayTime
        {
            get { return DateTimeHelper.ConvertTimeSpan(DisplayTimeStr); }
        }

        [JsonIgnore]
        public TimeSpan ExecutionTime
        {
            get { return DateTimeHelper.ConvertTimeSpan(ExecutionTimeStr); }
        }

        public List<UpdateExcerciseCommandModel> Excercises { get; set; } = new List<UpdateExcerciseCommandModel>();
    }
}