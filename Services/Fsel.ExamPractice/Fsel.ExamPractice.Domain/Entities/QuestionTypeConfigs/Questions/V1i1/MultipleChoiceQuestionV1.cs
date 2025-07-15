// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class MultipleChoiceQuestionV1
    {
        public IList<ConfigQuestionV1> Answers { get; set; } = new List<ConfigQuestionV1>();
    }
}
