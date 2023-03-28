using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Lesson : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Tên hiển thị
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Giáo Viên ID
        /// </summary>
        public Guid? TeacherId { get; set; }

        /// <summary>
        /// Trình độ khóa
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<LessonVideo> LessonVideos { get; set; } = new List<LessonVideo>();
        public ClassForum? ClassForum { get; set; }
        public ICollection<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();
        public ICollection<LessonExtraPractice> LessonExtraPractices { get; set; } = new List<LessonExtraPractice>();
        public ICollection<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
    }
}
