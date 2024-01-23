// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Helpers;

    public class MockTestAnswerModel : BaseAnswerModel
    {
        public Guid? SectionQuestionId { get; set; }
        public Guid MockTestResultId { get; set; }

        public int? TimeCount => MediaHelper.GetMediaDurationAsync(Answer?.ToString());
        public int? WordCount => StringHelper.CountWords(Answer?.ToString());

        public string? GradingAlFeedback { get; set; }
    }
}
