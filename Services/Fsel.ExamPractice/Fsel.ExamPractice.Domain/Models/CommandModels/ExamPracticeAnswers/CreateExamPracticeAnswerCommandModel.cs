// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAnswers
{
    public class CreateExamPracticeAnswerCommandModel
    {
        public Guid ExamPracticeResultId { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
        public bool IsSubmit { get; set; }
        public IList<ExamPracticeAnswerQuestionModel> Answers { get; set; } = new List<ExamPracticeAnswerQuestionModel>();
    }

    public class ExamPracticeAnswerQuestionModel
    {
        public Guid? QuestionId { get; set; }
        public Guid? ExamPracticeSectionId { get; set; }
        public object? Answer { get; set; }
        public string? SpeechTextAnswer { get; set; }
    }
}
