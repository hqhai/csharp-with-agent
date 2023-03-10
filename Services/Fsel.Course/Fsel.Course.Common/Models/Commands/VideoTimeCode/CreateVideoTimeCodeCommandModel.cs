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
using Fsel.Course.Common.Models.Commands.Videos;
using Fsel.Course.Common.Models.Commands.Excercise;
using Fsel.Common.Helpers;
using System.Text.Json.Serialization;

namespace Fsel.Course.Common.Models.Commands.VideoTimeCode
{
    public class CreateVideoTimeCodeCommandModel
    {
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

        public List<CreateExcerciseCommandModel> Excercises { get; set; } = new List<CreateExcerciseCommandModel>();
    }
}