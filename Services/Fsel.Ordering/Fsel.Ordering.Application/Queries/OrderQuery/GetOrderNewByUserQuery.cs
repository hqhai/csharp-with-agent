// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderNewByUserQuery : IRequest<MethodResult<bool>>
    {
    }

    public class GetOrderNewByUserQueryHandler : IRequestHandler<GetOrderNewByUserQuery, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;

        public GetOrderNewByUserQueryHandler(IOrderRepository orderRepository, AuthContext authContext)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(GetOrderNewByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId && x.Status == EnumOrderStatus.New, cancellationToken);
            methodResult.Result = order != null;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
