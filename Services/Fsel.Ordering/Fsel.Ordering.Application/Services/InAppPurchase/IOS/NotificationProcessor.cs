// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models;
    using MediatR;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;

    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<NotificationProcessor> _logger;
        private readonly IMediator _mediator;

        public NotificationProcessor(ILogger<NotificationProcessor> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<bool> Process(AppleNotification notification)
        {
            var v2Notification = GetVerifiedDecodedData<NotificationV2>(notification.SignedPayload);
            if (v2Notification?.DecodedPayload?.Data == null || !v2Notification.IsValid)
            {
                _logger.LogError("Data is null or is not valid");
                throw new ArgumentNullException($"{nameof(v2Notification.DecodedPayload.Data)} is null or is not valid");
            }

            RenewalInfoV2? renewalInfo = null;
            if (!string.IsNullOrEmpty(v2Notification.DecodedPayload.Data.SignedRenewalInfo))
            {
                var renewalInfoV2Verified = GetVerifiedDecodedData<RenewalInfoV2>(v2Notification.DecodedPayload.Data.SignedRenewalInfo);
                if (renewalInfoV2Verified.IsValid)
                    renewalInfo = renewalInfoV2Verified.DecodedPayload;
            }

            var transactionInfoResponse = GetVerifiedDecodedData<TransactionInfoV2>(v2Notification.DecodedPayload.Data.SignedTransactionInfo);
            TransactionInfoV2? transactionInfo = null;
            if (transactionInfoResponse.IsValid)
                transactionInfo = transactionInfoResponse.DecodedPayload;

            var response = new AppStoreResponseModel()
            {
                NotificationType = v2Notification.DecodedPayload?.NotificationType,
                Subtype = v2Notification.DecodedPayload?.Subtype,
                NotificationUUID = v2Notification.DecodedPayload?.NotificationUUID,
                NotificationVersion = v2Notification.DecodedPayload?.NotificationVersion,
                TransactionInfo = transactionInfo,
                RenewalInfoV2 = renewalInfo,
                SignedDate = v2Notification.DecodedPayload?.SignedDate
            };

            _logger.LogError(response.Serialize());

            var createOrderResult = await _mediator.Send(new NotificationWithAppStoreCommand() { DecodedPayload = v2Notification.DecodedPayload, RenewalInfo = renewalInfo, TransactionInfo = transactionInfo }).ConfigureAwait(false);
            if (createOrderResult.Result)
            {
                return true;
            }
            return false;
        }

        public TransactionInfoV2? TransactionInfo(string signedTransactionInfo)
        {
            var transactionInfoResponse = GetVerifiedDecodedData<TransactionInfoV2>(signedTransactionInfo);
            TransactionInfoV2? transactionInfo = null;
            if (transactionInfoResponse.IsValid)
                transactionInfo = transactionInfoResponse.DecodedPayload;
            return transactionInfo;
        }

        private VerifiedDecodedDataModel<TNotificationData> GetVerifiedDecodedData<TNotificationData>(string signedPayload)
        {
            if (string.IsNullOrEmpty(signedPayload))
            {
                _logger.LogError("Signed Payload is null");
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

        private void EnsurePartElements(string[] split)
        {
            if (split.Length != 3)
            {
                _logger.LogError("Invalid signedPayload");
                throw new ArgumentException("Invalid signedPayload");
            }

            if (string.IsNullOrEmpty(split[0]))
            {
                _logger.LogError("Invalid jws_header part");
                throw new ArgumentException("Invalid jws_header part");
            }

            if (string.IsNullOrEmpty(split[1]))
            {
                _logger.LogError("Invalid jws_payload part");
                throw new ArgumentException("Invalid jws_payload part");
            }

            if (string.IsNullOrEmpty(split[2]))
            {
                _logger.LogError("Invalid jws_signature part");
                throw new ArgumentException("Invalid jws_signature part");
            }
        }

        private bool VerifyToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);

                var x5cTry = jwtSecurityToken.Header.TryGetValue("x5c", out object x5cCerteficates);
                if (!x5cTry || x5cCerteficates == null)
                {
                    _logger.LogError("Token Header does not contain x5c");
                    throw new KeyNotFoundException("Token Header does not contain x5c");
                }

                var certeficatesItems = JsonConvert.DeserializeObject<IEnumerable<string>>(x5cCerteficates.ToString());
                if (certeficatesItems == null || !certeficatesItems.Any())
                {
                    _logger.LogError("Certeficates are null");
                    throw new ArgumentNullException("Certeficates are null");
                }

                var securityToken = Validate(handler, token, certeficatesItems.First());

                return securityToken != null;
            }
            catch (Exception ex)
            {
                // log it
                return false;
            }
        }

        private static SecurityToken? Validate(JwtSecurityTokenHandler tokenHandler, string jwtToken, string publicKey)
        {
            var certificateBytes = Base64UrlEncoder.DecodeBytes(publicKey);
            var certificate = new X509Certificate2(certificateBytes);
            var eCDsa = certificate.GetECDsaPublicKey();

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new ECDsaSecurityKey(eCDsa),
            };

            tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out var securityToken);
            return securityToken;
        }

        private static TObj DecodeFromBase64<TObj>(string encodedString)
        {
            var data = Base64UrlTextEncoder.Decode(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);

            var obj = JsonConvert.DeserializeObject<TObj>(decodedString);
            return obj;
        }
    }
}
