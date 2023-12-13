// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Infrastructure.Common;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class PaymentCommand : PaymentCommandModel, IRequest<MethodResult<string>>
    {
    }

    public class PaymentCommandHandler : IRequestHandler<PaymentCommand, MethodResult<string>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly VnPayLibrary _vnPayLibrary;

        public PaymentCommandHandler(IOrderRepository orderRepository, VnPayLibrary vnPayLibrary)
        {
            _orderRepository = orderRepository;
            _vnPayLibrary = vnPayLibrary;
        }

        public async Task<MethodResult<string>> Handle(PaymentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string>();

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.OrderCode, cancellationToken);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var price = (long)order.TotalPrice * 100;

            methodResult.Result = _vnPayLibrary.CreateRequestUrl(order.Code ?? string.Empty, price, order.CreatedDate);
            return methodResult;
        }
    }
}
