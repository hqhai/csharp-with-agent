// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class HomeWork : Entity
    {
        /// <summary>
        /// Tên bài tập
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        ///  Code
        /// </summary>
        ///
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        public string? MediaPost { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// Loại
        /// </summary>
        public EnumHomeWorkType Type { get; set; }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        /// <summary>
        /// Trạng thái Archive
        /// </summary>
        public bool IsArchive { get; set; }

        public ICollection<HomeWorkResult> HomeWorkResults { get; set; } = new List<HomeWorkResult>();
        public ICollection<LessonHomeWork> LessonHomeWorks { get; set; } = new List<LessonHomeWork>();
        public ICollection<HomeWorkQuestion> HomeWorkQuestions { get; set; } = new List<HomeWorkQuestion>();
        public ICollection<HomeWorkConfig> HomeWorkConfigs { get; set; } = new List<HomeWorkConfig>();
        public ICollection<HomeWorkExtraPracticeResult> HomeWorkExtraPracticeResults { get; set; } = new List<HomeWorkExtraPracticeResult>();
        public ICollection<HomeWorkRetry> HomeWorkRetrys { get; set; } = new List<HomeWorkRetry>();
    }
}
