using Fsel.Common.Helpers;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums
{
    public class UpdateClassForumCommandModel : BaseCommandModel
    {
        public string? Title { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }
        public string? TaggetTimeLimitStr { get; set; }

        [JsonIgnore]
        public TimeSpan TaggetTimeLimit
        {
            get { return DateTimeHelper.ConvertTimeSpan(TaggetTimeLimitStr); }
        }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
    }
}
