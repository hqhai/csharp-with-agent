using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAnnotationsExtensions;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class ClassForum : Entity
    {
        /// <summary>
        /// Nội dung
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        /// <summary>
        /// Cách chấm điểm
        /// </summary>
        public EnumGradingStyle GradingStyle { get; set; }

        /// <summary>
        /// Số từ giới hạn
        /// </summary>
        [Min(1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long TaggetWordLimit { get; set; }

        /// <summary>
        /// Thời gian giới hạn
        /// </summary>
        [Min(1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long TaggetTimeLimit { get; set; }

        [NotMapped]
        public TimeSpan TaggetTimeSpanLimit
        {
            get { return TimeSpan.FromTicks(TaggetTimeLimit); }
        }

        /// <summary>
        /// Media Post
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public Guid LessonId { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
