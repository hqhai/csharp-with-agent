// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChangeStatusOderCommand : ChangeStatusOrderCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeStatusOderCommandHandler : IRequestHandler<ChangeStatusOderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        public ChangeStatusOderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusOderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var order = await _orderRepository.GetByIdAsync(request.OderId);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OderNotExist));
                return methodResult;
            }
            if (order.Status != EnumOrderStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OderStatusIsNotNew));
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order.Status = request.OderStatus;
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
