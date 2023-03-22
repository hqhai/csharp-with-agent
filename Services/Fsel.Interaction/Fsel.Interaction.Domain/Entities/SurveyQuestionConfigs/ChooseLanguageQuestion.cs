// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class ChooseLanguageQuestion
    {
        public IList<ChooseLanguageQuestions>? ChooseLanguageQuestions { get; set; }
    }

    public class ChooseLanguageQuestions
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
    }

    public class ChooseLanguageAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class ChooseLanguageAnswer
    {
        public IList<ChooseLanguageAnswers>? ChooseLanguageAnswers { get; set; }
    }
}
