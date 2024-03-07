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
        public EnumTypeOfError Type { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public EnumPriority? Priority { get; set; }

        public EnumErrorReportStatus Status { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Url { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBack { get; set; }

        public EnumFeaturePlatForm? FeaturePlatform { get; set; }

        public EnumFeatureLearn? FeatureLearn { get; set; }
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ImagePathsStr { get; set; }

        [NotMapped]
        public IList<string>? ImagePaths
        {
            get { return ConvertHelper.Deserialize<IList<string>>(ImagePathsStr); }
            set { ImagePathsStr = ConvertHelper.Serialize(value); }
        }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }
    }
}
