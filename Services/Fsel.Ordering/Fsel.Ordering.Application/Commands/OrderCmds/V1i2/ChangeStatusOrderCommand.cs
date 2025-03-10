// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ChangeStatusOrderCommand : ChangeStatusOrderCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeStatusOrderCommandHandler : IRequestHandler<ChangeStatusOrderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        private readonly IMediator _mediator;

        public ChangeStatusOrderCommandHandler(IOrderRepository orderRepository, IMediator mediator)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (request.OrderIds == null || request.OrderIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var orders = await _orderRepository.Queryable.Where(p => request.OrderIds.Contains(p.Id)).ToListAsync(cancellationToken);
            if (orders.Count != request.OrderIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (request.Status == EnumOrderStatus.Payment && !request.RevenueType.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var item in orders)
            {
                var result = await _mediator.Send(new OrderCmds.ChangeStatusOrderCommand()
                {
                    OrderId = item.Id,
                    OrderStatus = request.Status,
                    RevenueType = request.RevenueType,
                    Receipt = request.Serialize()
                }, cancellationToken);
                if (!result.IsOK)
                {
                    methodResult.AddError(result.ErrorMessages);
                    return methodResult;
                }
            }
            return methodResult;
        }
    }
}
