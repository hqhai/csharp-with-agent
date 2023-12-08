// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Infrastructure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Http;

    public class VnPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            string? retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
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
                string vnp_SecureHash = Utils.HmacSHA512(vnpHashSecret, queryString);

                return $"{baseUrl}?{queryString}&{PaymentSetting.VNPay.VnpSecureHash}={vnp_SecureHash}";
            }

            return baseUrl;
        }

        #endregion Request

        #region Response process

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetResponseData();
            string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
        }

        private string GetResponseData()
        {
            var filteredData = _responseData
    .Where(kv => !string.IsNullOrEmpty(kv.Value) &&
                 kv.Key != PaymentSetting.VNPay.VnpSecureHashType &&
                 kv.Key != PaymentSetting.VNPay.VnpSecureHash)
    .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}");

            if (filteredData.Any())
            {
                return string.Join("&", filteredData);
            }
            return string.Empty;
        }

        #endregion Response process
    }

    public static class Utils
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return hash.ToString();
        }

        public static string? GetIpAddress(IHttpContextAccessor httpContextAccessor)
        {
            string? ipAddress;
            try
            {
                var xForwardedForHeader = httpContextAccessor?.HttpContext?.Request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(xForwardedForHeader))
                {
                    ipAddress = xForwardedForHeader;

                    // Kiểm tra và lấy IP đầu tiên nếu có nhiều địa chỉ IP được chuyển tiếp bởi proxy
                    if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(',', StringComparison.CurrentCulture))
                    {
                        ipAddress = ipAddress.Split(',')[0].Trim();
                    }
                }
                else
                {
                    // Lấy địa chỉ IP thực sự nếu không có proxy hoặc giá trị từ proxy không hợp lệ
                    ipAddress = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và gán giá trị mặc định nếu có lỗi
                ipAddress = "Invalid IP: " + ex.Message;
            }

            return ipAddress;
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
