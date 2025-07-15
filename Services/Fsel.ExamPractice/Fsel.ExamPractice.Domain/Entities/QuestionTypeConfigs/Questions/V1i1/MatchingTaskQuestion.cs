// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class MatchingTaskQuestion
    {
        public string? Content { get; set; }
        public IList<ConfigAnswerV1> Placeholders { get; set; } = new List<ConfigAnswerV1>();
        public IList<ConfigAnswerV1> Answers { get; set; } = new List<ConfigAnswerV1>();
    }
}
