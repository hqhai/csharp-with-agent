using Fsel.Core.Base.BaseModels;

namespace Fsel.Identity.Domain.Models
{
    public class StudentEventLearningRecordModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public Guid CompetitionEventId { get; set; }
        public int TotalLearningDays { get; set; }
        public int TotalLessons { get; set; }
        public int TotalQuestBoards { get; set; }
        public int TotalTokens { get; set; }
        public int TotalVocabulary { get; set; }
        public int TotalReading { get; set; }
        public int TotalListening { get; set; }
        public int TotalGrammar { get; set; }
        public int TotalWriting { get; set; }
        public int TotalSpeaking { get; set; }
        public bool IsView { get; set; }
    }
}
