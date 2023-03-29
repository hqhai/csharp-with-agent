using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Course : Entity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Số lượng Unit
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int NumberOfUnits { get; set; }

        /// <summary>
        /// Số Lượng Lesson
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int NumberOfLessons { get; set; }

        /// <summary>
        /// Loại trạng thái
        /// </summary>
        public EnumCourseStatus Status { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public ICollection<CourseClassStudent> CourseClassStudents { get; set; } = new List<CourseClassStudent>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
    }
}
