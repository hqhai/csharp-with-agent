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
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderTrialQuery : IRequest<MethodResult<OrderModel?>>
    {
    }

    public class GetOrderTrialQueryHandler : IRequestHandler<GetOrderTrialQuery, MethodResult<OrderModel?>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetOrderTrialQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<OrderModel?>> Handle(GetOrderTrialQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel?>();

            var order = await _orderRepository.Queryable.Where(p => p.UserId == _authContext.CurrentUserId && p.IsTrial).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = _mapper.Map<OrderModel?>(order);
            return methodResult;
        }
    }
}
