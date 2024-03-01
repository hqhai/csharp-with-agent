// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;

    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ISubscriptionService _subscriptionService;

        public NotificationProcessor(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public void Process(AppleNotification notification)
        {
            var v2Notification = GetVerifiedDecodedData<NotificationV2>(notification?.SignedPayload);
            if (v2Notification?.DecodedPayload?.Data == null || !v2Notification.IsValid)
            {
                throw new ArgumentNullException($"{nameof(v2Notification.DecodedPayload.Data)} is null or is not valid");
            }

            RenewalInfoV2? renewalInfo = null;
            if (!string.IsNullOrEmpty(v2Notification.DecodedPayload.Data.SignedRenewalInfo))
            {
                var renewalInfoV2Verified = GetVerifiedDecodedData<RenewalInfoV2>(v2Notification.DecodedPayload.Data.SignedRenewalInfo);
                if (renewalInfoV2Verified.IsValid)
                {
                    renewalInfo = renewalInfoV2Verified.DecodedPayload;
                }
            }

            var transactionInfoResponse = GetVerifiedDecodedData<TransactionInfoV2>(v2Notification.DecodedPayload.Data.SignedTransactionInfo);
            TransactionInfoV2? transactionInfo = null;
            if (transactionInfoResponse.IsValid)
            {
                transactionInfo = transactionInfoResponse.DecodedPayload;
            }

            // Update Internal subscription
            _subscriptionService.Update(v2Notification.DecodedPayload, renewalInfo, transactionInfo);
        }

        private static VerifiedDecodedDataModel<TNotificationData> GetVerifiedDecodedData<TNotificationData>(string? signedPayload)
        {
            if (string.IsNullOrEmpty(signedPayload))
            {
                throw new ArgumentNullException("Signed Payload is null");
            }

            var splitParts = signedPayload.Split('.'); // JWS header, payload, and signature representations

            EnsurePartElements(splitParts);

            var valid = VerifyToken(signedPayload);

            var payload = splitParts[1];

            return new VerifiedDecodedDataModel<TNotificationData>
            {
                DecodedPayload = valid ? DecodeFromBase64<TNotificationData>(payload) : default,
                IsValid = valid
            };
        }

        private static void EnsurePartElements(string[] split)
        {
            if (split.Length != 3)
            {
                throw new ArgumentException("Invalid signedPayload");
            }

            if (string.IsNullOrEmpty(split[0]))
            {
                throw new ArgumentException("Invalid jws_header part");
            }

            if (string.IsNullOrEmpty(split[1]))
            {
                throw new ArgumentException("Invalid jws_payload part");
            }

            if (string.IsNullOrEmpty(split[2]))
            {
                throw new ArgumentException("Invalid jws_signature part");
            }
        }

        private static bool VerifyToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);

                var x5cTry = jwtSecurityToken.Header.TryGetValue("x5c", out object? x5cCertificates);
                if (!x5cTry || x5cCertificates == null)
                {
                    throw new KeyNotFoundException("Token Header does not contain x5c");
                }

                var certificatesItems = JsonConvert.DeserializeObject<IEnumerable<string>>(x5cCertificates.ToString());
                if (certificatesItems == null || !certificatesItems.Any())
                {
                    throw new ArgumentNullException("Certeficates are null");
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

        private static TObj? DecodeFromBase64<TObj>(string encodedString)
        {
            var data = Base64UrlTextEncoder.Decode(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);

            var obj = JsonConvert.DeserializeObject<TObj>(decodedString);
            return obj;
        }
    }
}
