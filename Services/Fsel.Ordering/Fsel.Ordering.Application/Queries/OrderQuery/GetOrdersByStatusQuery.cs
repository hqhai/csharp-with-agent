// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOrdersByStatusQuery : IRequest<MethodResult<IList<OrderModel?>>>
    {
        public EnumOrderStatus Status { get; set; }
    }

    public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, MethodResult<IList<OrderModel?>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<OrderModel?>>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderModel?>>();

            var order = await _orderRepository.Queryable.Where(p => p.UserId == _authContext.CurrentUserId && p.Status == request.Status).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<OrderModel?>>(order);
            return methodResult;
        }
    }
}
