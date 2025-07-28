namespace Fsel.Course.Lms.Application.Services.SystemService.QueryModels
{
    public class GetFeatureAccessTimesByUserIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
