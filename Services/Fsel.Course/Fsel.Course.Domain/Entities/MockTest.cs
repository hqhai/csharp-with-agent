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
    public class MockTest : Entity
    {
        [Required(ErrorMessage = nameof(EnumMockTestErrorCode.MT01V))]
        [MaxLength(250, ErrorMessage = nameof(EnumMockTestErrorCode.MT01C))]
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumMockTestType MockTestType { get; set; }

        public List<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();

        public List<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();
    }
}