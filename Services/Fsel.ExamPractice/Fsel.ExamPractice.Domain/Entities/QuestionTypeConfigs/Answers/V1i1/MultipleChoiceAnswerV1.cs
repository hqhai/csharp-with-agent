// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers.V1i1
{
    public class MultipleChoiceAnswerV1
    {
        public IList<ConfigAnswer> Answers { get; set; } = new List<ConfigAnswer>();
    }
}
