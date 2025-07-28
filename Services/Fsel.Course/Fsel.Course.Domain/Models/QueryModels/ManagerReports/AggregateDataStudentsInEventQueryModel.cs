namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    public class AggregateDataStudentsInEventQueryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
