// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Shared.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Entities
{
    public class Course : Entity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Mã khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn khóa học
        /// </summary>
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

        public ICollection<CourseUnitMockTest> CourseUnitMockTests { get; set; } = new List<CourseUnitMockTest>();
        public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public CourseResult? CourseResult { get; set; }
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();

        public ICollection<MockTestResult> MockTestResults { get; set; } = new List<MockTestResult>();
    }
}
