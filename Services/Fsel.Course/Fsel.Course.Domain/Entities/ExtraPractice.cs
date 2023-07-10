// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class ExtraPractice : Entity
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tên thực hành bổ sung
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Nội dung thực hành bổ sung
        /// </summary>
        public string? InstructionContent { get; set; }

        /// <summary>
        /// Đường dẫn tệp
        /// </summary>
        public string? BookFilePath { get; set; }
        public string? BookCoverPath { get; set; }
        public string? BookBackgroundPath { get; set; }

        /// <summary>
        /// Tác giả
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Nội dung
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Abstract { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// VideoLink
        /// </summary>
        public string? VideoLink { get; set; }

        /// <summary>
        /// Loại ExtraPractice
        /// </summary>
        public EnumExtraPracticeType Type { get; set; }

        /// <summary>
        /// Trình độ Level
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        public Guid? VideoId { get; set; }
        public Video? Video { get; set; }
        public Guid? PlacementTestId { get; set; }
        public PlacementTest? PlacementTest { get; set; }
        public Guid? MockTestId { get; set; }
        public MockTest? MockTest { get; set; }
        public ICollection<LessonExtraPractice> LessonExtraPractices { get; set; } = new List<LessonExtraPractice>();
        public ICollection<ExtraPracticeChapter> ExtraPracticeChapters { get; set; } = new List<ExtraPracticeChapter>();
        public ICollection<ExtraPracticeExercise> ExtraPracticeExercises { get; set; } = new List<ExtraPracticeExercise>();
    }
}
