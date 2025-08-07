namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersHasOrderPaymentQuery : IRequest<MethodResult<IList<Guid>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetUsersHasOrderPaymentQueryHandler : IRequestHandler<GetUsersHasOrderPaymentQuery, MethodResult<IList<Guid>>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetUsersHasOrderPaymentQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetUsersHasOrderPaymentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<Guid>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var userIds = await _orderRepository.Queryable.WhereBulkContains(request.UserIds, p => p.UserId).Where(p => p.Status == Shared.Enums.EnumOrderStatus.Payment).Select(p => p.UserId).ToListAsync(cancellationToken);

            methodResult.Result = userIds;
            return methodResult;
        }
    }
}
