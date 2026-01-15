// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Attributes;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Entities.TestConfigs;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Entities
{
    public class Course : Entity, IVersionEntity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        /// <summary>
        /// Mã khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[^<>]*$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Code { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn khóa học
        /// </summary>
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Loại trạng thái
        /// </summary>
        public EnumCourseStatus Status { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Loại khóa học
        /// </summary>
        [NotMapped]
        public EnumCourseType CourseType
        {
            get
            {
                return CourseLevel.GetEnumCourseType();
            }
        }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public Guid? ParentCourseId { get; set; }

        public int Priority { get; set; }

        public int UnitCount { get; set; }

        public int TestCount { get; set; }

        public Guid OriginalId { get; set; }

        public int Version { get; set; }

        public EnumVersionStatus VersionStatus { get; set; }

        public Guid? LevelId { get; set; }

        public Level? Level { get; set; }

        public Guid? ProgramId { get; set; }

        public Category? Program { get; set; }

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public ICollection<CourseResult> CourseResults { get; set; } = new List<CourseResult>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
        public ICollection<MockTestResult> MockTestResults { get; set; } = new List<MockTestResult>();
        public ICollection<FinalTestResult> FinalTestResults { get; set; } = new List<FinalTestResult>();
        public ICollection<CourseModule> CourseModules { get; set; } = new List<CourseModule>();
        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
    }
}
