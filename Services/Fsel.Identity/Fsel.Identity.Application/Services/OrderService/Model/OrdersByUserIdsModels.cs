namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class OrdersByUserIdsModels
    {
        public IList<OrdersByUserIdsModel>? Users { get; set; }
    }

    public class OrdersByUserIdsModel
    {
        public Guid UserId { get; set; }
        public IList<OrderModel>? Orders { get; set; }
    }
}
