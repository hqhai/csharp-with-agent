namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class StudentCompetitionEventModel
    {
        public Guid StudentId { get; set; }
        public Guid UserId { get; set; }
        public Guid CompetitionEventId { get; set; }
    }
}
