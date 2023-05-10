// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class ChooseMultipleColumnQuestion
    {
        public IList<ChooseDirectionQuestionAnswers>? ChooseDirectionQuestions { get; set; }
    }

    public class ChooseDirectionQuestionAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class ChooseDirectionAnswer
    {
        public ChooseDirectionQuestionAnswers? ChooseDirectionAnswers { get; set; }
    }
}
