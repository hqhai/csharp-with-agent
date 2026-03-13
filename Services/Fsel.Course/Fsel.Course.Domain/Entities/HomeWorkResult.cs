// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class HomeWorkResult : BaseScoreResult, ITokenResult, ISubmissionCount, IHighestStreak
    {
        private EnumSubmissionCount? _submissionCount;
        public int? HighestStreak { get; set; }
        public int? HighestStreakSubQuestion { get; set; }

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

        public Guid HomeWorkId { get; set; }
        public HomeWork? HomeWork { get; set; }
        public Guid LessonResultId { get; set; }
        public LessonResult? LessonResult { get; set; }

        public LessonModule? LessonModule { get; set; }
        public Guid? LessonModuleId { get; set; }
        public ICollection<HomeWorkAnswer> HomeWorkAnswers { get; set; } = new List<HomeWorkAnswer>();
    }
}
