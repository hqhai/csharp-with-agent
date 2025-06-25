namespace Fsel.System.Domain.Models.QueryModels
{
    public class AggregateDataStudentsInEventQueryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<AggregateDataStudentInEventQueryModel>? Students { get; set; }
    }

    public class AggregateDataStudentInEventQueryModel
    {
        public Guid StudentId { get; set; }
        public Guid UserId { get; set; }
    }
}
