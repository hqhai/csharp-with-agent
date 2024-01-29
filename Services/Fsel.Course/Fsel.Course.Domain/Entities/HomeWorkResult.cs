// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public class HomeWorkResult : BaseScoreResult
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HomeWorkId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        public HomeWork? HomeWork { get; set; }
        public EnumSubmissionCount SubmissionCount { get; set; }
        public LessonResult? LessonResult { get; set; }

        public ICollection<HomeWorkAnswer> HomeWorkAnswers { get; set; } = new List<HomeWorkAnswer>();
    }
}
