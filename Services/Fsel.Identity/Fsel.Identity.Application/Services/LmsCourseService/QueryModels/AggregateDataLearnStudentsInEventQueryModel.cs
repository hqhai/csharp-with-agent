namespace Fsel.Identity.Application.Services.LmsCourseService.QueryModels
{
    public class AggregateDataLearnStudentsInEventQueryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}
