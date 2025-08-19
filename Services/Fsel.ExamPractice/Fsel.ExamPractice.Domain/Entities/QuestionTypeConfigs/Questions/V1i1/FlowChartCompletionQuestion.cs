// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class FlowChartCompletionQuestion
    {
        public IList<ConfigContentQuestionV1> Contents { get; set; } = new List<ConfigContentQuestionV1>();
        public IList<ConfigAnswerV1> Answers { get; set; } = new List<ConfigAnswerV1>();
    }
}
