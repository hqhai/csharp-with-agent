namespace Fsel.Ordering.Application.Services.UserService.Models
{
    public class UpdateExpiredDateForStudentsEventCommandModel
    {
        public IList<Guid>? StudentIds { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
