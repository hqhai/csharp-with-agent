// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.QuestionAnswers
{
    public class StudyTimeQuestion
    {
        public IList<StudyTimeQuestions>? StudyTimeQuestions { get; set; }
    }

    public class StudyTimeQuestions
    {
        public int Id { get; set; }
        public string? Content { get; set; }
    }

    public class StudyTimeAnswer
    {
        public StudyTimeQuestions? StudyTimeQuestions { get; set; }
    }
}
