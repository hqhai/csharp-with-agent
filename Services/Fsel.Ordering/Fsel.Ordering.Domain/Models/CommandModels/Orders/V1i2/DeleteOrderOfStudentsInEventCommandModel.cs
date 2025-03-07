namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    public class DeleteOrderOfStudentsInEventCommandModel
    {
        public IList<Guid>? UserIds { get; set; }
    }
}
