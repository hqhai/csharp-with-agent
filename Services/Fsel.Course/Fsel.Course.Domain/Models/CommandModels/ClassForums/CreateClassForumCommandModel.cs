using System.Text.Json.Serialization;
using Fsel.Common.Helpers;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.ClassForums
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
