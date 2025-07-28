namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;

    public class GetOrdersByUserIdsQuery : IRequest<MethodResult<OrdersByUserIdsModels>>
    {
        public IList<Guid>? UserIds { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }

    public class GetOrdersByUserIdsQueryHandler : IRequestHandler<GetOrdersByUserIdsQuery, MethodResult<OrdersByUserIdsModels>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetOrdersByUserIdsQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<OrdersByUserIdsModels>> Handle(GetOrdersByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrdersByUserIdsModels>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var orders = _orderRepository.Queryable.WhereBulkContains(request.UserIds, p => p.UserId);

            if (request.Status.HasValue)
            {
                orders = orders.Where(p => p.Status == request.Status);
            }

            if (request.Status.HasValue)
            {
                orders = orders.Where(p => p.RevenueType == request.RevenueType);
            }

            var users = request.UserIds.Select(p => new OrdersByUserIdsModel
            {
                UserId = p,
                Orders = _mapper.Map<IList<OrderModel>>(orders.Where(x => x.UserId == p))
            }).ToList();

            methodResult.Result = new OrdersByUserIdsModels
            {
                Users = users
            };

            return methodResult;
        }
    }
}
