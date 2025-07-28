namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    public class AggregateDataOtherStudentsInEventQueryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<AggregateDataOtherStudentInEventQueryModel>? Students { get; set; }
    }

    public class AggregateDataOtherStudentInEventQueryModel
    {
        public Guid StudentId { get; set; }
        public Guid UserId { get; set; }
    }
}
