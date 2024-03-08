// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;

    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<NotificationProcessor> _logger;

        public NotificationProcessor(ISubscriptionService subscriptionService, ILogger<NotificationProcessor> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        public void Process(AppleNotification notification)
        {
            _logger.LogError(notification.SignedPayload);
            var v2Notification = GetVerifiedDecodedData<NotificationV2>(notification?.SignedPayload);
            if (v2Notification?.DecodedPayload?.Data == null || !v2Notification.IsValid)
            {
                _logger.LogError("Data is null or is not valid");
                return;
            }

            RenewalInfoV2? renewalInfo = null;
            if (!string.IsNullOrEmpty(v2Notification.DecodedPayload.Data.SignedRenewalInfo))
            {
                var renewalInfoV2Verified = GetVerifiedDecodedData<RenewalInfoV2>(v2Notification.DecodedPayload.Data.SignedRenewalInfo);
                if (renewalInfoV2Verified != null && renewalInfoV2Verified.IsValid)
                {
                    renewalInfo = renewalInfoV2Verified.DecodedPayload;
                }
            }

            var transactionInfoResponse = GetVerifiedDecodedData<TransactionInfoV2>(v2Notification.DecodedPayload.Data.SignedTransactionInfo);
            TransactionInfoV2? transactionInfo = null;
            if (transactionInfoResponse != null && transactionInfoResponse.IsValid)
            {
                transactionInfo = transactionInfoResponse.DecodedPayload;
            }
            _logger.LogError("Done");
            _subscriptionService.Update(v2Notification.DecodedPayload, renewalInfo, transactionInfo);
        }

        private VerifiedDecodedDataModel<TNotificationData>? GetVerifiedDecodedData<TNotificationData>(string? signedPayload)
        {
            if (string.IsNullOrEmpty(signedPayload))
            {
                _logger.LogError("Signed Payload is null");
                return null;
            }

            var splitParts = signedPayload.Split('.'); // JWS header, payload, and signature representations

            var ensurePartElements = EnsurePartElements(splitParts);
            if (!ensurePartElements)
            {
                return null;
            }

            var valid = VerifyToken(signedPayload);
            if (!valid)
            {
                return null;
            }

            var payload = splitParts[1];

            return new VerifiedDecodedDataModel<TNotificationData>
            {
                DecodedPayload = valid ? DecodeFromBase64<TNotificationData>(payload) : default,
                IsValid = valid
            };
        }

        private bool EnsurePartElements(string[] split)
        {
            if (split.Length != 3)
            {
                _logger.LogError("Invalid signedPayload");
                return false;
            }

            if (string.IsNullOrEmpty(split[0]))
            {
                _logger.LogError("Invalid jws_header part");
                return false;
            }

            if (string.IsNullOrEmpty(split[1]))
            {
                _logger.LogError("Invalid jws_payload part");
                return false;
            }

            if (string.IsNullOrEmpty(split[2]))
            {
                _logger.LogError("Invalid jws_signature part");
                return false;
            }
            return true;
        }

        private bool VerifyToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);

                var x5cTry = jwtSecurityToken.Header.TryGetValue("x5c", out object? x5cCertificates);
                if (!x5cTry || x5cCertificates == null)
                {
                    _logger.LogError("Token Header does not contain x5c");
                    return false;
                }

                var certificatesItems = JsonConvert.DeserializeObject<IEnumerable<string>>(x5cCertificates.ToString());
                if (certificatesItems == null || !certificatesItems.Any())
                {
                    _logger.LogError("Certeficates are null");
                    return false;
                }

                var securityToken = Validate(handler, token, certificatesItems.First());

                return securityToken != null;
            }
            catch
            {
                return false;
            }
        }

        private static SecurityToken? Validate(JwtSecurityTokenHandler tokenHandler, string jwtToken, string publicKey)
        {
            try
            {
                var certificateBytes = Base64UrlEncoder.DecodeBytes(publicKey);
                using (var certificate = new X509Certificate2(certificateBytes))
                {
                    var eCDsa = certificate.GetECDsaPublicKey();

                    TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new ECDsaSecurityKey(eCDsa),
                    };

                    tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out var securityToken);
                    return securityToken;
                }
            }
            catch
            {
                return null;
            }
        }

        private static TObj? DecodeFromBase64<TObj>(string encodedString)
        {
            var data = Base64UrlTextEncoder.Decode(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);

            var obj = JsonConvert.DeserializeObject<TObj>(decodedString);
            return obj;
        }
    }
}
