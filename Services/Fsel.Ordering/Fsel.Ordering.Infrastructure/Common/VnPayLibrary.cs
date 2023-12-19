// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Common
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;

    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private readonly AppSetting? _appSetting;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public VnPayLibrary(AppSetting? appSetting, IHttpContextAccessor contextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _appSetting = appSetting;
            _contextAccessor = contextAccessor;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        #region Request

        public string CreateRequestUrl(string code, long price, DateTime createdDate)
        {
            var protector = _dataProtectionProvider.CreateProtector(_appSetting?.Jwt?.SecretKey ?? string.Empty);
            var token = protector.Protect(code ?? string.Empty);

            var vnpReturnurl = _appSetting?.ConstantUrl?.PaymentSuccessUrl;
            string vnp_Returnurl = string.Format(CultureInfo.InvariantCulture, vnpReturnurl ?? string.Empty, token);

            var ipAddress = IpAddress();

            if (string.IsNullOrEmpty(vnp_Returnurl) || string.IsNullOrEmpty(ipAddress))
            {
                return string.Empty;
            }

            AddData(vnp_Returnurl, ipAddress, price, createdDate, code ?? string.Empty);

            string? baseUrl = _appSetting?.PaymentConfig?.VNPAY?.VnpUrl;
            string? vnpHashSecret = _appSetting?.PaymentConfig?.VNPAY?.VnpHashSecret;

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(vnpHashSecret))
            {
                return string.Empty;
            }

            var encodedPairs = _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}");

            if (encodedPairs.Any())
            {
                string queryString = string.Join("&", encodedPairs);
                string vnp_SecureHash = EncodeHelper.HmacSHA512(vnpHashSecret, queryString);

                return $"{baseUrl}?{queryString}&{PaymentSetting.VNPay.VnpSecureHash}={vnp_SecureHash}";
            }

            return baseUrl;
        }

        #endregion Request

        private void AddData(string vnp_Returnurl, string ipAddress, long price, DateTime createdDate, string code)
        {
            AddRequestData(PaymentSetting.VNPay.VnpVersion, _appSetting?.PaymentConfig?.VNPAY?.Version ?? string.Empty);
            AddRequestData(PaymentSetting.VNPay.VnpCommand, "pay");
            AddRequestData(PaymentSetting.VNPay.VnpTmnCode, _appSetting?.PaymentConfig?.VNPAY?.VnpTmnCode ?? string.Empty);
            AddRequestData(PaymentSetting.VNPay.VnpCurrCode, "VND");
            AddRequestData(PaymentSetting.VNPay.VnpIpAddr, ipAddress);
            AddRequestData(PaymentSetting.VNPay.VnpLocale, "vn");
            AddRequestData(PaymentSetting.VNPay.VnpOrderType, "other");
            AddRequestData(PaymentSetting.VNPay.VnpReturnUrl, vnp_Returnurl);
            AddRequestData(PaymentSetting.VNPay.VnpCreateDate, createdDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            AddRequestData(PaymentSetting.VNPay.VnpAmount, price.ToString(CultureInfo.CurrentCulture));
            AddRequestData(PaymentSetting.VNPay.VnpOrderInfo, "Thanh toán đơn hàng :" + code);
            AddRequestData(PaymentSetting.VNPay.VnpTxnRef, code);
        }

        private string? IpAddress()
        {
            return _contextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
