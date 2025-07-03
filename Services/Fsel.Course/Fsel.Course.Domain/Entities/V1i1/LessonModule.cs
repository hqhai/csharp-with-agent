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
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
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

        public Guid? LessonId { get; set; }

        public Lesson? Lesson { get; set; }

        public Guid? VideoId { get; set; }

        public Video? Video { get; set; }

        public Guid? ClassForumId { get; set; }

        public ClassForum? ClassForum { get; set; }

        public Guid? HomeWorkId { get; set; }

        public HomeWork? HomeWork { get; set; }

        public Guid? DocumentId { get; set; }
        public Document? Document { get; set; }
    }
}
