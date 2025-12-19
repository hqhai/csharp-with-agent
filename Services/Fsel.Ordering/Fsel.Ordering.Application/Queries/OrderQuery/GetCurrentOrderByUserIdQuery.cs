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
        private readonly IPackageEventRepository _packageEventRepository;

        public GetCurrentOrderByUserIdQueryHandler(IOrderRepository orderRepository,
            IMapper mapper,
            IPackageEventRepository packageEventRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _packageEventRepository = packageEventRepository;
        }

        public async Task<MethodResult<OrderModel>> Handle(GetCurrentOrderByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();
            var order = await _orderRepository.Queryable.Where(p => p.UserId == request.UserId)
                                              .OrderByDescending(p => p.CreatedDate)
                                              .FirstOrDefaultAsync(cancellationToken);

            var orderModel = _mapper.Map<OrderModel>(order);

            if (order != null)
            {
                var packageEvent = await _packageEventRepository.Queryable.Include(x => x.Event)
                                                      .FirstOrDefaultAsync(x => x.PackageId == order.PackageId, cancellationToken);

                if (packageEvent != null)
                {
                    orderModel.IsDefault = packageEvent.Event != null && packageEvent.Event.IsDefault;
                }
            }
            methodResult.Result = _mapper.Map<OrderModel>(order);
            return methodResult;
        }
    }
}
