// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.QuestionAnswers
{
    public class ChooseDirectionQuestion
    {
        public IList<ChooseDirectionQuestions>? ChooseDirectionQuestions { get; set; }
    }

    public class ChooseDirectionQuestions
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class ChooseDirectionAnswer
    {
        public ChooseDirectionQuestions? ChooseDirectionQuestions { get; set; }
    }
}
