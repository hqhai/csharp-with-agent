using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Course : Entity
    {
        [Required(ErrorMessage = nameof(EnumCourseErrorCode.C01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumCourseErrorCode.C02C))]
        public string? Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumCourseErrorCode.C04C))]
        public int NumberOfUnits { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumCourseErrorCode.C04C))]
        public int NumberOfLessons { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
    }
}
