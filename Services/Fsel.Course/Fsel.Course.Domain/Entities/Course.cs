using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Course : Entity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumCourseErrorCode.C01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumCourseErrorCode.C02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Số lượng Unit
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumCourseErrorCode.C03C))]
        public int NumberOfUnits { get; set; }

        /// <summary>
        /// Số Lượng Lesson
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumCourseErrorCode.C04C))]
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
    }
}
