// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStatusOrderByUserQuery : IsCheckPaymentStatusByUserModel, IRequest<MethodResult<bool>>
    {
    }

    public class GetStatusOrderByUserQueryHandler : IRequestHandler<GetStatusOrderByUserQuery, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetStatusOrderByUserQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<bool>> Handle(GetStatusOrderByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            //var order = await _orderRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PackageId == request.PackageId && x.ClassId == request.ClassId && x.CourseId == request.CourseId, cancellationToken);

            #region pilot

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.ClassId == request.ClassId && x.CourseId == request.CourseId, cancellationToken);

            #endregion pilot

            if (order == null)
            {
                methodResult.Result = false;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            methodResult.Result = order.Status == EnumOrderStatus.Payment;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
