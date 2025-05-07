namespace Fsel.System.Domain.Models.EntityModels
{
    public class DailyQuizModel
    {
        public string? Code { get; set; }
        public int NumberQuestion { get; set; }
        public int NumberCorrect { get; set; }
        public bool IsDone { get; set; }
        public IList<DailyQuizQuestionModel>? DailyQuizQuestions { get; set; }
    }
}
