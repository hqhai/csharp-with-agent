// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class UnitLesson : Entity
    {
        /// <summary>
        /// Số thứ tự
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DisplayOrder { get; set; }

        public Lesson? Lesson { get; set; }
        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitId { get; set; }

        public ICollection<LessonResult> UnitLessonResults { get; set; } = new List<LessonResult>();
    }
}
