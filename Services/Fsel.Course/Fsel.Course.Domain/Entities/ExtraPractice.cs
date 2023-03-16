// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class ExtraPractice : Entity
    {
        /// <summary>
        /// Tên ExtraPractice
        /// </summary>
        [Required(ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung ExtraPractice
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP03C))]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// File Link
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP04C))]
        public string? FilePath { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Loại ExtraPractice
        /// </summary>
        public EnumExtraPracticeType Type { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public ICollection<LessonExtraPractice> LessonExtraPractices { get; set; } = new List<LessonExtraPractice>();
    }
}
