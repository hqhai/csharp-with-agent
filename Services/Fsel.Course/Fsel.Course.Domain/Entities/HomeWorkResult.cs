// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class HomeWorkResult : BaseScoreResult, ITokenResult, ISubmissionCount
    {
        private EnumSubmissionCount? _submissionCount;

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HomeWorkId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        public HomeWork? HomeWork { get; set; }
        public LessonResult? LessonResult { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public EnumSubmissionCount? SubmissionCount
        {
            get
            {
                return _submissionCount.HasValue ? _submissionCount : EnumSubmissionCount.FirstSubmit;
            }
            set { _submissionCount = value; }
        }

        public ICollection<HomeWorkAnswer> HomeWorkAnswers { get; set; } = new List<HomeWorkAnswer>();
    }
}
