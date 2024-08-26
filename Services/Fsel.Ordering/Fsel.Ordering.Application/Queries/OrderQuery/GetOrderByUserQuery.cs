// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderByUserQuery : IRequest<MethodResult<OrderModel>>
    {
    }

    public class GetOrderByUserQueryHandler : IRequestHandler<GetOrderByUserQuery, MethodResult<OrderModel>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;

        public GetOrderByUserQueryHandler(IOrderRepository orderRepository, AuthContext authContext, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<OrderModel>> Handle(GetOrderByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();
            var order = await _orderRepository.Queryable.Where(p => p.UserId == _authContext.CurrentUserId).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = _mapper.Map<OrderModel>(order);
            return methodResult;
        }
    }
}
