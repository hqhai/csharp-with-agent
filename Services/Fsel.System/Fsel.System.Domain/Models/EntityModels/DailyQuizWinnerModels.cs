namespace Fsel.System.Domain.Models.EntityModels
{
    public class DailyQuizWinnerModels
    {
        public string? Code { get; set; }
        public IList<DailyQuizWinnerModel>? Winner { get; set; }
    }

    public class DailyQuizWinnerModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? School { get; set; }
        public string? Code { get; set; }
    }
}
