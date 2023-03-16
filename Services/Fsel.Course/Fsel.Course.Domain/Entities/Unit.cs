using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class Unit : Entity
    {
        /// <summary>
        /// Tên Unit
        /// </summary>
        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Tên hiển thị
        /// </summary>
        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U03C))]
        [MaxLength(250, ErrorMessage = nameof(EnumUnitErrorCode.U04C))]
        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Loại Unit
        /// </summary>
        public EnumUnitType Type { get; set; }

        /// <summary>
        /// Trình dộ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
    }
}
