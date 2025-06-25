// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.InAppPurchase.Android;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.InAppPurchases.Androids;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class VerifyDataFromAndroidAppCommand : VerifyDataFromAndroidAppCommandModel, IRequest<MethodResult<object>>
    {
    }

    public class VerifyDataFromAndroidAppCommandHandler : IRequestHandler<VerifyDataFromAndroidAppCommand, MethodResult<object>>
    {
        private readonly IGooglePlayBillingService _service;
        private readonly ILogger<VerifyDataFromAndroidAppCommand> _logger;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly IOrderRepository _orderRepository;
        private const string SubscriptionState = "SUBSCRIPTION_STATE_ACTIVE";
        private const string AcknowledgementState = "ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED";

        public VerifyDataFromAndroidAppCommandHandler(IGooglePlayBillingService service, ILogger<VerifyDataFromAndroidAppCommand> logger, IMediator mediator, AuthContext authContext, IOrderRepository orderRepository)
        {
            _service = service;
            _logger = logger;
            _mediator = mediator;
            _authContext = authContext;
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<object>> Handle(VerifyDataFromAndroidAppCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<object>();

            var requestStr = request.Serialize();
            _logger.LogError($"Request: {requestStr}");

            if (string.IsNullOrEmpty(request.PackageName) || string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.SubscriptionId))
            {
                _logger.LogError($"DataNotExist: {request.PackageName} {request.Token} {request.SubscriptionId}");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var subscriptionPurchase = await _service.VerifySubscriptionAsync(request.PackageName, request.Token);
            if (subscriptionPurchase == null)
            {
                _logger.LogError($"subscriptionPurchase is null");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var subscriptionPurchaseStr = subscriptionPurchase.Serialize();
            _logger.LogInformation(subscriptionPurchaseStr);

            if (subscriptionPurchase.SubscriptionState != SubscriptionState && subscriptionPurchase.AcknowledgementState != AcknowledgementState)
            {
                _logger.LogError($"subscriptionPurchase.SubscriptionState({subscriptionPurchase.SubscriptionState}) != {SubscriptionState} or  subscriptionPurchase.AcknowledgementState{subscriptionPurchase.AcknowledgementState} != {AcknowledgementState}");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (subscriptionPurchase.LatestOrderId != request.PurchaseId)
            {
                _logger.LogError($"subscriptionPurchase.LatestOrderId({subscriptionPurchase.LatestOrderId}) != request.PurchaseId({request.PurchaseId})");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Id == request.OrderId && p.UserId == _authContext.CurrentUserId, cancellationToken);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(order));
                return methodResult;
            }

            if (order.Status != EnumOrderStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OrderStatusIsNotNew));
                return methodResult;
            }

            var result = await _mediator.Send(new OrderCmds.ChangeStatusOrderCommand()
            {
                OrderId = request.OrderId,
                OrderStatus = EnumOrderStatus.Payment,
                Type = EnumOrderTransactionType.GooglePlay,
                Receipt = subscriptionPurchaseStr,
                RevenueType = EnumPaymentRevenueType.Revenue
            }, cancellationToken);

            if (!result.IsOK)
            {
                methodResult.AddError(result.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = subscriptionPurchase;
            return methodResult;
        }
    }
}
