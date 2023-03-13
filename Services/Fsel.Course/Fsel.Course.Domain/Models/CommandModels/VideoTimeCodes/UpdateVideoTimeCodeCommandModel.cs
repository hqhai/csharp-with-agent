using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Models.CommandModels.Excercises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
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
            get { return DisplayTimeStr.ConvertTimeSpan(); }
        }

        [JsonIgnore]
        public TimeSpan ExecutionTime
        {
            get { return ExecutionTimeStr.ConvertTimeSpan(); }
        }

        public List<UpdateExcerciseCommandModel> Excercises { get; set; } = new List<UpdateExcerciseCommandModel>();
    }
}
