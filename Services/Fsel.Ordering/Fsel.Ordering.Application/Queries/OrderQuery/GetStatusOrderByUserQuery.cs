// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStatusOrderByUserQuery : GetOrderStatusByUserModel, IRequest<MethodResult<EnumOrderStatus?>>
    {
    }

    public class GetStatusOrderByUserQueryHandler : IRequestHandler<GetStatusOrderByUserQuery, MethodResult<EnumOrderStatus?>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;

        public GetStatusOrderByUserQueryHandler(IOrderRepository orderRepository, AuthContext authContext)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<EnumOrderStatus?>> Handle(GetStatusOrderByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<EnumOrderStatus?> methodResult = new MethodResult<EnumOrderStatus?>();
            //var order = await _orderRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PackageId == request.PackageId && x.ClassId == request.ClassId && x.CourseId == request.CourseId, cancellationToken);

            #region pilot

            var query = _orderRepository.Queryable.Where(x => (!request.UserId.HasValue || x.UserId == request.UserId) || x.UserId == _authContext.CurrentUserId);

            #endregion pilot

            if (request.ClassId.HasValue)
            {
                query = query.Where(x => x.ClassId == request.ClassId);
            }
            if (request.CourseId.HasValue)
            {
                query = query.Where(x => x.CourseId == request.CourseId);
            }
            if (request.PackageId.HasValue)
            {
                query = query.Where(x => x.PackageId == request.PackageId);
            }
            var order = await query.FirstOrDefaultAsync(cancellationToken);
            methodResult.Result = order?.Status;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
