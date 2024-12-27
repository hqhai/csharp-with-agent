// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.InAppPurchase.Android;
    using Fsel.Ordering.Domain.Models.CommandModels.InAppPurchases.Androids;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class VerifyDataFromAndroidAppCommand : VerifyDataFromAndroidAppCommandModel, IRequest<MethodResult<object>>
    {
    }

    public class VerifyDataFromAndroidAppCommandHandler : IRequestHandler<VerifyDataFromAndroidAppCommand, MethodResult<object>>
    {
        private readonly IGooglePlayBillingService _service;
        private readonly ILogger<VerifyDataFromAndroidAppCommand> _logger;
        private readonly IMediator _mediator;

        public VerifyDataFromAndroidAppCommandHandler(IGooglePlayBillingService service, ILogger<VerifyDataFromAndroidAppCommand> logger, IMediator mediator)
        {
            _service = service;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<MethodResult<object>> Handle(VerifyDataFromAndroidAppCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<object>();

            if (string.IsNullOrEmpty(request.PackageName) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SubscriptionId))
            {
                _logger.LogError($"DataNotExist: {request.PackageName} {request.Token} {request.SubscriptionId}");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var subscriptionPurchase = await _service.VerifySubscriptionAsync(request.PackageName, request.SubscriptionId, request.Token);
            if (subscriptionPurchase == null)
            {
                _logger.LogError($"subscriptionPurchase is null");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var subscriptionPurchaseStr = subscriptionPurchase.Serialize();
            _logger.LogInformation(subscriptionPurchaseStr);
            if (!subscriptionPurchase.PaymentState.HasValue || subscriptionPurchase.PaymentState != 1)
            {
                _logger.LogError($"subscriptionPurchase.PaymentState is null or != 1");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (subscriptionPurchase.ExpiryTimeMillis <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                _logger.LogError($"subscriptionPurchase.ExpiryTimeMillis <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var subscriptionPurchaseJson = subscriptionPurchase.Serialize();
            _logger.LogInformation(subscriptionPurchaseJson);
            await _mediator.Send(new OrderCmds.ChangeStatusOrderCommand()
            {
                OrderId = new Guid(subscriptionPurchase.OrderId),
                OrderStatus = EnumOrderStatus.Payment,
                Type = EnumOrderTransactionType.GooglePlay,
                Receipt = subscriptionPurchaseJson,
                RevenueType = EnumPaymentRevenueType.Revenue
            }, cancellationToken);
            methodResult.Result = subscriptionPurchase;
            return methodResult;
        }
    }
}
