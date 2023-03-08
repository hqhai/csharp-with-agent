using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class ClassForum : Entity
    {
        [Required(ErrorMessage = nameof(EnumClassForumErrorCode.CF01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumClassForumErrorCode.CF02C))]
        public string? Title { get; set; }

        public EnumGradingStyle GradingStyle { get; set; }

        public TimeSpan? TaggetTimeLimit { get; set; }
        public TimeSpan? TaggetWordLimit { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumClassForumErrorCode.CF03C))]
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
        public Guid? LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}