using Fsel.Shared.Enums;

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class GetOrdersByUserIdsQueryModel
    {
        public IList<Guid>? UserIds { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}
