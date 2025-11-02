// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentOrderByUserIdQuery : IRequest<MethodResult<OrderModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetCurrentOrderByUserIdQueryHandler : IRequestHandler<GetCurrentOrderByUserIdQuery, MethodResult<OrderModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetCurrentOrderByUserIdQueryHandler(IOrderRepository orderRepository,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<OrderModel>> Handle(GetCurrentOrderByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();
            var order = await _orderRepository.Queryable.Where(p => p.UserId == request.UserId)
                                              .OrderByDescending(p => p.CreatedDate)
                                              .FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<OrderModel>(order);
            return methodResult;
        }
    }
}
