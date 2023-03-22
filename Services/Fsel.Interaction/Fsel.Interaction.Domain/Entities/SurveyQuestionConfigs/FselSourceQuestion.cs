// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class FselSourceQuestion
    {
        public IList<FselSourceQuestions>? FselSourceQuestions { get; set; }
    }

    public class FselSourceQuestions
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
    }

    public class FselSourceAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class FselSourceAnswer
    {
        public IList<FselSourceAnswers>? FselSourceAnswers { get; set; }
    }
}
