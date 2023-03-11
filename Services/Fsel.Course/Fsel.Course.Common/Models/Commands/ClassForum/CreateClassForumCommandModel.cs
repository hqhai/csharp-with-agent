using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.ClassForum
{
    public class CreateClassForumCommandModel
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