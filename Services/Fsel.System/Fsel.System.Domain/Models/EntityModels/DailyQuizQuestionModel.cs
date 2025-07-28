namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class DailyQuizQuestionModel : BaseModel
    {
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public string? Content { get; set; }
        public string? Explanation { get; set; }
        public IList<DailyQuizAnswerModel>? DailyQuizAnswers { get; set; }
    }

    public class DailyQuizAnswerModel : BaseModel
    {
        public string? Content { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsChoice { get; set; }
        public Guid DailyQuizQuestionId { get; set; }
    }
}
