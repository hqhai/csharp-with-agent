// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Common
{
    using System.Collections.Generic;
    using System.Net;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;

    public class VnPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        #region Request

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var encodedPairs = _requestData
    .Where(kv => !string.IsNullOrEmpty(kv.Value))
    .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}");

            if (encodedPairs.Any())
            {
                string queryString = string.Join("&", encodedPairs);
                string vnp_SecureHash = EncodeHelper.HmacSHA512(vnpHashSecret, queryString);

                return $"{baseUrl}?{queryString}&{PaymentSetting.VNPay.VnpSecureHash}={vnp_SecureHash}";
            }

            return baseUrl;
        }

        #endregion Request
    }
}
