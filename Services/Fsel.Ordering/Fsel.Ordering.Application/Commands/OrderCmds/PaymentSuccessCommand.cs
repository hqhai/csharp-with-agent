// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class PaymentSuccessCommand : IRequest<MethodResult<bool>>
    {
        public string? SecretKey { get; set; }
    }

    public class PaymentSuccessCommandHandler : IRequestHandler<PaymentSuccessCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public PaymentSuccessCommandHandler(IOrderRepository orderRepository, IMediator mediator, AppSetting appSetting, IDataProtectionProvider dataProtectionProvider)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
            _appSetting = appSetting;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public async Task<MethodResult<bool>> Handle(PaymentSuccessCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var protector = _dataProtectionProvider.CreateProtector(_appSetting.Jwt?.SecretKey ?? string.Empty);
            string orderCode;
            try
            {
                orderCode = protector.Unprotect(request.SecretKey ?? string.Empty);
            }
            catch
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.TokenExpired));
                return methodResult;
            }

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Code == orderCode, cancellationToken);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (order.Status != EnumOrderStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OrderStatusIsNotNew));
                return methodResult;
            }

            var changeStatusOrder = await _mediator.Send(new ChangeStatusOrderCommand { OrderId = order.Id, OrderStatus = EnumOrderStatus.Payment, PackageId = order.PackageId }, cancellationToken).ConfigureAwait(false);
            if (!changeStatusOrder.IsOK)
            {
                methodResult.AddError(changeStatusOrder.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
