namespace Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2
{
    using Fsel.Ordering.Domain.Models.EntityModels;

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
