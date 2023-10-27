// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class LessonExtraPractice : Entity
    {
        public Lesson? Lesson { get; set; }
        public ExtraPractice? ExtraPractice { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ExtracPraticeId { get; set; }
    }
}
