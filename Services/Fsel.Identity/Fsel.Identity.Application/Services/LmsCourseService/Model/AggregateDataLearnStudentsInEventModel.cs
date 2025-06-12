namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class AggregateDataLearnStudentsInEventModel
    {
        public Guid StudentId { get; set; }
        public int TotalLesson { get; set; }
        public int TotalReading { get; set; }
        public int TotalListening { get; set; }
        public int TotalWriting { get; set; }
        public int TotalSpeaking { get; set; }
        public int TotalVocabulary { get; set; }
        public int TotalGrammar { get; set; }
    }
}
