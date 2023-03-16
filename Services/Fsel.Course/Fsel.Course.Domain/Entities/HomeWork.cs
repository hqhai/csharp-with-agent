// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class HomeWork : Entity
    {
        /// <summary>
        /// Tên bài tập
        /// </summary>
        [Required(ErrorMessage = nameof(EnumHomeWorkErrorCode.HW01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW02C))]
        public string? Name { get; set; }

        /// <summary>
        ///  Nội dung bài tập
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW03C))]
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumHomeWorkErrorCode.HW04C))]
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public ICollection<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();
    }
}
