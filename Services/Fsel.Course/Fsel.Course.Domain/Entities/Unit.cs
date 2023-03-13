using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Unit : Entity
    {
        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U02C))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U02C))]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public EnumUnitType Type { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public List<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();

        public List<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public List<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
    }
}
