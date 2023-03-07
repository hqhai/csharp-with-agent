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
    public class MockFinalTest : Entity
    {
        [Required(ErrorMessage = nameof(EnumMockFinalTestErrorCode.MFT01V))]
        [MaxLength(250, ErrorMessage = nameof(EnumMockFinalTestErrorCode.MFT01C))]
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public List<UnitMockFinalTest> UnitMockFinalTests { get; set; } = new List<UnitMockFinalTest>();
    }
}