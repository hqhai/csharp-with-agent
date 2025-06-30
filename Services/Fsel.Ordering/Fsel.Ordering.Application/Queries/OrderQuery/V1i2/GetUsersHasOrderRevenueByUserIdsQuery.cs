namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersHasOrderRevenueByUserIdsQuery : IRequest<MethodResult<IList<OrderModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetUsersHasOrderRevenueByUserIdsQueryHandler : IRequestHandler<GetUsersHasOrderRevenueByUserIdsQuery, MethodResult<IList<OrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetUsersHasOrderRevenueByUserIdsQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<OrderModel>>> Handle(GetUsersHasOrderRevenueByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderModel>>();
            var orders = await _orderRepository.Queryable.WhereBulkContains(request.UserIds, p => p.UserId).Where(x => x.Status == EnumOrderStatus.Payment && x.RevenueType == EnumPaymentRevenueType.Revenue && !x.IsTrial).Include(p => p.Package).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);
            methodResult.Result = _mapper.Map<IList<OrderModel>>(orders);
            return methodResult;
        }
    }
}
