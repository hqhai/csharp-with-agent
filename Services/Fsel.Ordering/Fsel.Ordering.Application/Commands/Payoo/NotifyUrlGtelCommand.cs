// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class NotifyUrlGtelCommand : NotifyUrlCommandModel, IRequest<MethodResult<NotifyUrlModel>>
    {
    }

    public class NotifyUrlGtelCommandHandler : IRequestHandler<NotifyUrlGtelCommand, MethodResult<NotifyUrlModel>>
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger<NotifyUrlGtelCommand> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public NotifyUrlGtelCommandHandler(AppSetting appSetting,
            ILogger<NotifyUrlGtelCommand> logger,
            IOrderRepository orderRepository,
            IMediator mediator)
        {
            _appSetting = appSetting;
            _logger = logger;
            _orderRepository = orderRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<NotifyUrlModel>> Handle(NotifyUrlGtelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<NotifyUrlModel>();

            var payooResponse = request.Serialize();

            _logger.LogError($"payoo notify: {payooResponse}");

            methodResult.Result = new NotifyUrlModel { ReturnCode = 1, Description = string.Empty };

            var secureHash = EncodeHelper.SecureHash(_appSetting.PayooGtelConfig?.Key + request.ResponseData + _appSetting.PayooGtelConfig?.PayooIP);
            if (secureHash.ToLower(CultureInfo.CurrentCulture) != request.SecureHash?.ToLower(CultureInfo.CurrentCulture))
            {
                _logger.LogError($"SecureHash wrong: {request.SecureHash}");
                return methodResult;
            }

            var paymentInfo = request.ResponseData.Deserialize<PaymentInfoResponseModel>();
            var paymentInfoStr = paymentInfo.Serialize();
            _logger.LogError($"Payment Info: {paymentInfoStr}");
            if (paymentInfo == null || string.IsNullOrEmpty(paymentInfo.OrderNo))
            {
                _logger.LogError($"Payment Info Null: {paymentInfoStr}");
                return methodResult;
            }

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Code.ToLower() == paymentInfo.OrderNo.ToLower(), cancellationToken);
            if (order == null)
            {
                _logger.LogError($"Order Null: {paymentInfo.OrderNo}");
                return methodResult;
            }

            await _mediator.Send(new ChangeStatusOrderCommand()
            {
                OrderId = order.Id,
                OrderStatus = paymentInfo.PaymentStatus == 1 ? EnumOrderStatus.Payment : EnumOrderStatus.Fail,
                Type = EnumOrderTransactionType.Payoo,
                Receipt = paymentInfo.Serialize(),
                RevenueType = EnumPaymentRevenueType.Revenue
            }, cancellationToken);

            methodResult.Result = new NotifyUrlModel { ReturnCode = 0, Description = string.Empty };
            return methodResult;
        }
    }
}
