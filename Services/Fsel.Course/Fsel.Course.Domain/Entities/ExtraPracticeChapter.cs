// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ExtraPracticeChapter : Entity
    {
        /// <summary>
        /// Tên thực hành bổ sung
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Sự miêu tả
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public int PageNumber { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }

        public Guid ExtraPracticeId { get; set; }
        public ICollection<ExtraPracticeExercise> ExtraPracticeExercises { get; set; } = new List<ExtraPracticeExercise>();
    }
}
