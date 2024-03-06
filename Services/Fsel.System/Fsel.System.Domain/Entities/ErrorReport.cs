// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.System.Domain.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class ErrorReport : Entity
    {
        public EnumTypeOfError TypeOfError { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public EnumPriority? Priority { get; set; }

        public EnumErrorReportStatus ReportStatus { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Url { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FselFeedBack { get; set; }

        public EnumPlatFormDetail? PlatFormDetail { get; set; }

        public EnumLessonDetail? LessonDetail { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ImageLinksStr { get; set; }

        [NotMapped]
        public IList<string>? ImageLinks
        {
            get { return ConvertHelper.Deserialize<IList<string>>(ImageLinksStr); }
            set { ImageLinksStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? StudentFeedBack { get; set; }
    }
}
