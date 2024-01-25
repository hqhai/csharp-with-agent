// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class HomeWorkResultModel : BaseScoreResultModel
    {
        public Guid HomeWorkId { get; set; }
        public Guid LessonResultId { get; set; }
        public EnumSubmissionCount SubmissionCount { get; set; }
    }
}
