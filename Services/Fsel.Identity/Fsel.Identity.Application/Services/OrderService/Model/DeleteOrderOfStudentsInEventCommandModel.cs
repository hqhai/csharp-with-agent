namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class DeleteOrderOfStudentsInEventCommandModel
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
