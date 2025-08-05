// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;

    public class LessonModule : Entity, IDisplayInfo
    {
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Thumbnail { get; set; }

        public EnumLessonConfigType LessonConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Guid LessonId { get; set; }

        public Lesson? Lesson { get; set; }

        public Guid OriginalId { get; set; }
    }
}
