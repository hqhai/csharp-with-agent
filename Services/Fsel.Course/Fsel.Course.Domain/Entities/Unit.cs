// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Unit : Entity
    {
        /// <summary>
        /// Mã Unit
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tên Unit
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Trình dộ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public ICollection<UnitSkillMockTest> UnitSkillMockTests { get; set; } = new List<UnitSkillMockTest>();
        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<UnitLesson> UnitLessons { get; set; } = new List<UnitLesson>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
        public ICollection<MockTestResult> MockTestResults { get; set; } = new List<MockTestResult>();
    }
}
