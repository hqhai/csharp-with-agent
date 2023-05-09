// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class YourDirectionQuestion
    {
        public IList<FselSourceQuestionAnswers>? FselSourceQuestions { get; set; }
    }

    public class FselSourceQuestionAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
    }

    public class FselSourceAnswer
    {
        public FselSourceQuestionAnswers? FselSourceAnswers { get; set; }
    }
}
