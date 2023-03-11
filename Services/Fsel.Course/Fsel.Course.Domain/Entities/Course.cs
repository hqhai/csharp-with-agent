using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using System.ComponentModel.DataAnnotations;

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

        public bool IsPublish { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public List<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
    }
}