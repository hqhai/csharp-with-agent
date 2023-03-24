// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class StudyTimeQuestion
    {
        public IList<StudyTimeQuestionAnswers>? StudyTimeQuestions { get; set; }
    }

    public class StudyTimeQuestionAnswers
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class StudyTimeAnswer
    {
        public StudyTimeQuestionAnswers? StudyTimeAnswers { get; set; }
    }
}
