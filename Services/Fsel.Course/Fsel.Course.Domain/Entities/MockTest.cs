// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class MockTest : Entity
    {
        /// <summary>
        /// Tên MockTest
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Loại MockTest
        /// </summary>
        public EnumMockTestType MockTestType { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        [NotMapped]
        public double ExecutionTime
        {
            get
            {
                return MockTestSections.Where(x => x.SectionGroup is not null).Sum(x => x!.SectionGroup!.ExecutionTime);
            }
        }

        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double Version { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }
        public ICollection<MockTestSection> MockTestSections { get; set; } = new List<MockTestSection>();
        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<MockTestResult> MockTestResults { get; set; } = new List<MockTestResult>();
        public ICollection<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();
    }
}
