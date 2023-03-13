using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Common.Helpers;
using System.Text.Json.Serialization;
using Fsel.Course.Domain.Models.CommandModels.Excercises;

namespace Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes
{
    public class CreateVideoTimeCodeCommandModel
    {
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

        public List<CreateExcerciseCommandModel> Excercises { get; set; } = new List<CreateExcerciseCommandModel>();
    }
}
