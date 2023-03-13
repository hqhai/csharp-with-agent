using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class ClassForum : Entity
    {
        [Required(ErrorMessage = nameof(EnumClassForumErrorCode.CF01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumClassForumErrorCode.CF02C))]
        public string? Title { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumClassForumErrorCode.CF04C))]
        public long TaggetWordLimit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumClassForumErrorCode.CF04C))]
        public long TaggetTimeLimitTicks { get; set; }

        [NotMapped]
        public TimeSpan TaggetTimeLimit
        {
            get { return TimeSpan.FromTicks(TaggetTimeLimitTicks); }
            set { TaggetTimeLimitTicks = value.Ticks; }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumClassForumErrorCode.CF03C))]
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
        public Guid LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
