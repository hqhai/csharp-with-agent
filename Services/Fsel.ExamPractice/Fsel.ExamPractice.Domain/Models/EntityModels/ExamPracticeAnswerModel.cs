// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using Fsel.ExamPractice.Domain.Models.EntityModels.Bases;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;

    public class ExamPracticeAnswerModel : BaseAnswerModel
    {
        public Guid? ExamPracticeSectionId { get; set; }
        public Guid ExamPracticeResultId { get; set; }
        public int? TimeCount { get; set; }
        public int? WordCount { get; set; }
        public string? SpeechTextAnswer { get; set; }
        public IList<AiFeedbackItemModel>? GradingAlFeedbacks { get; set; } = new List<AiFeedbackItemModel>();
    }
}
