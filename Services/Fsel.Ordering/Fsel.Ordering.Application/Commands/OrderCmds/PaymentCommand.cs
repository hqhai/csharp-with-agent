// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class PaymentCommand : PaymentCommandModel, IRequest<MethodResult<string>>
    {
    }

    public class PaymentCommandHandler : IRequestHandler<PaymentCommand, MethodResult<string>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppSetting _appSetting;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public PaymentCommandHandler(IOrderRepository orderRepository, IHttpContextAccessor contextAccessor, AppSetting appSetting, IDataProtectionProvider dataProtectionProvider)
        {
            _orderRepository = orderRepository;
            _contextAccessor = contextAccessor;
            _appSetting = appSetting;
            _dataProtectionProvider = dataProtectionProvider;
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

            var protector = _dataProtectionProvider.CreateProtector(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var token = protector.Protect(request.OrderCode ?? string.Empty);

            var vnpReturnurl = _appSetting.ConstantUrl?.PaymentSuccessUrl;

            if (string.IsNullOrEmpty(vnpReturnurl))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            string vnp_Returnurl = string.Format(CultureInfo.InvariantCulture, vnpReturnurl, token);

            string? vnp_Url = _appSetting.VNPAY?.VnpUrl;
            string? vnp_TmnCode = _appSetting.VNPAY?.VnpTmnCode;
            string? vnp_HashSecret = _appSetting.VNPAY?.VnpHashSecret;

            if (string.IsNullOrEmpty(vnp_Url) || string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            VnPayLibrary vnpay = new VnPayLibrary();

            var price = (long)order.TotalPrice * 100;

            vnpay.AddRequestData(PaymentSetting.VNPay.VnpVersion, VnPayLibrary.VERSION);
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpCommand, "pay");
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpTmnCode, vnp_TmnCode);
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpAmount, price.ToString(CultureInfo.CurrentCulture));

            if (request.VNPAYPaymentType.HasValue)
            {
                vnpay.AddRequestData(PaymentSetting.VNPay.VnpBankCode, request.VNPAYPaymentType.ToString() ?? string.Empty);
            }

            var ipAddress = Utils.GetIpAddress(_contextAccessor);

            if (string.IsNullOrEmpty(ipAddress) || ipAddress.Contains("Invalid IP", StringComparison.CurrentCulture))
            {
                methodResult.AddErrorBadRequest(ipAddress);
                return methodResult;
            }

            vnpay.AddRequestData(PaymentSetting.VNPay.VnpCreateDate, order.CreatedDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpCurrCode, "VND");
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpIpAddr, ipAddress);
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpLocale, "vn");
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpOrderInfo, "Thanh toán đơn hàng :" + order.Code);
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpOrderType, "other"); //default value: other

            vnpay.AddRequestData(PaymentSetting.VNPay.VnpReturnUrl, vnp_Returnurl);
            vnpay.AddRequestData(PaymentSetting.VNPay.VnpTxnRef, order.Code ?? string.Empty);

            methodResult.Result = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return methodResult;
        }
    }
}
