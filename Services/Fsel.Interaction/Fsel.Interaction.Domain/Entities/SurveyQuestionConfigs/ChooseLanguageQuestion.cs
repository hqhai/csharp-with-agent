// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class ChooseLanguageQuestion
    {
        public IList<ChooseLanguageQuestionAnswers>? ChooseLanguageQuestions { get; set; }
    }

    public class ChooseLanguageQuestionAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
    }

    public class ChooseLanguageAnswer
    {
        public ChooseLanguageQuestionAnswers? ChooseLanguageAnswers { get; set; }
    }
}
