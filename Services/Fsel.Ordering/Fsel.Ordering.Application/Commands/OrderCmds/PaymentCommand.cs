// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Infrastructure.Common;
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
        private readonly VnPayLibrary _vnPayLibrary;

        public PaymentCommandHandler(IOrderRepository orderRepository, IHttpContextAccessor contextAccessor, AppSetting appSetting, IDataProtectionProvider dataProtectionProvider, VnPayLibrary vnPayLibrary)
        {
            _orderRepository = orderRepository;
            _contextAccessor = contextAccessor;
            _appSetting = appSetting;
            _dataProtectionProvider = dataProtectionProvider;
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

            var protector = _dataProtectionProvider.CreateProtector(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var token = protector.Protect(request.OrderCode ?? string.Empty);

            var vnpReturnurl = _appSetting.ConstantUrl?.PaymentSuccessUrl;

            if (string.IsNullOrEmpty(vnpReturnurl))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            string vnp_Returnurl = string.Format(CultureInfo.InvariantCulture, vnpReturnurl, token);

            string? vnp_Url = _appSetting.PaymentConfig?.VNPAY?.VnpUrl;
            string? vnp_TmnCode = _appSetting.PaymentConfig?.VNPAY?.VnpTmnCode;
            string? vnp_HashSecret = _appSetting.PaymentConfig?.VNPAY?.VnpHashSecret;

            if (string.IsNullOrEmpty(vnp_Url) || string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var price = (long)order.TotalPrice * 100;

            var version = _appSetting.PaymentConfig?.VNPAY?.Version;
            if (string.IsNullOrEmpty(version))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var ipAddress = _contextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                methodResult.AddErrorBadRequest(ipAddress);
                return methodResult;
            }

            AddRequestData(version, vnp_TmnCode, price, order.CreatedDate, ipAddress, order.Code ?? string.Empty, vnp_Returnurl);

            methodResult.Result = _vnPayLibrary.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return methodResult;
        }

        private void AddRequestData(string version, string vnp_TmnCode, long price, DateTime createdDate, string ipAddress, string code, string vnp_Returnurl)
        {
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpVersion, version);
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpCommand, "pay");
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpTmnCode, vnp_TmnCode);
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpAmount, price.ToString(CultureInfo.CurrentCulture));
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpCreateDate, createdDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpCurrCode, "VND");
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpIpAddr, ipAddress);
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpLocale, "vn");
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpOrderInfo, "Thanh toán đơn hàng :" + code);
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpOrderType, "other");
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpReturnUrl, vnp_Returnurl);
            _vnPayLibrary.AddRequestData(PaymentSetting.VNPay.VnpTxnRef, code);
        }
    }
}
