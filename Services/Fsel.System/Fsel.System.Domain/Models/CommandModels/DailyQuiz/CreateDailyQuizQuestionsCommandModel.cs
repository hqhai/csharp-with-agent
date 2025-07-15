namespace Fsel.System.Domain.Models.CommandModels.DailyQuiz
{
    public class CreateDailyQuizQuestionsCommandModel
    {
        public IList<CreateDailyQuizQuestionCommandModel>? DailyQuizQuestions { get; set; }
    }

    public class CreateDailyQuizQuestionCommandModel
    {
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Content { get; set; }
        public string? Explanation { get; set; }
        public IList<CreateDailyQuizAnswerCommandModel>? DailyQuizAnswers { get; set; }
        public IList<CreateDailyQuizTranslationCommandModel>? Translations { get; set; }
    }

    public class CreateDailyQuizAnswerCommandModel
    {
        public string? Content { get; set; }
        public bool IsCorrect { get; set; }
        public IList<CreateDailyQuizTranslationCommandModel>? Translations { get; set; }
    }

    public class CreateDailyQuizTranslationCommandModel
    {
        public string? Content { get; set; }
        public string? Explanation { get; set; }
        public string? Language { get; set; }
    }
}
