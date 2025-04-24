namespace Fsel.System.Domain.Models.EntityModels
{
    public class DailyQuizModel
    {
        public DateTime SpinTime { get; set; }
        public string? Code { get; set; }
        public int NumberQuestion { get; set; }
        public int NumberCorrect { get; set; }
        public bool IsDone { get; set; }
        public IList<DailyQuizQuestionModel>? DailyQuizQuestions { get; set; }
    }
}
