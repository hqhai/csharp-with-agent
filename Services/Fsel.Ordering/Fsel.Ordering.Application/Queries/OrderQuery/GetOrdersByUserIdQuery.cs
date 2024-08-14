// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOrdersByUserIdQuery : IRequest<MethodResult<IList<OrderModel>>>
    {
        public Guid? UserId { get; set; }
        public EnumOrderStatus? Status { get; set; }
    }

    public class GetOrdersByUserIdQueryHandler : IRequestHandler<GetOrdersByUserIdQuery, MethodResult<IList<OrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetOrdersByUserIdQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<OrderModel>>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderModel>>();

            var userId = request.UserId ?? _authContext.CurrentUserId;

            var order = await _orderRepository.Queryable.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            if (request.Status.HasValue)
            {
                order = order.Where(p => p.Status == request.Status).ToList();
            }

            methodResult.Result = _mapper.Map<IList<OrderModel>>(order);
            return methodResult;
        }
    }
}
