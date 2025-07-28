namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    public class UpdateExpiredDateForStudentsEventCommandModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
