// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class ExtraPractice : Entity
    {
        [Required(ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP02C))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung hướng dẫn bài test
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP03C))]
        public string? InstructionContent { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumExtraPractiveErrorCode.EP03C))]
        public string? FilePath { get; set; }

        public bool IsActive { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public List<LessonExtraPractice> LessonExtraPractices { get; set; } = new List<LessonExtraPractice>();
    }
}
