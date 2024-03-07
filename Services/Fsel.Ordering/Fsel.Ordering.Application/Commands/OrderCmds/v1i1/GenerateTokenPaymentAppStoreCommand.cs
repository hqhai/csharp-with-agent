// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.InAppPurchase;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Jose;
    using MediatR;

    public class GenerateTokenPaymentAppStoreCommand : IRequest<MethodResult<object>>
    {
    }

    public class GenerateTokenPaymentAppStoreCommandHandler : IRequestHandler<GenerateTokenPaymentAppStoreCommand, MethodResult<object>>
    {
        private readonly AppSetting appSetting;
        private readonly IInAppPurchaseService _inAppPurchaseService;
        private readonly IMediator _mediator;

        public GenerateTokenPaymentAppStoreCommandHandler(AppSetting appSetting, IInAppPurchaseService inAppPurchaseService, IMediator mediator)
        {
            this.appSetting = appSetting;
            _inAppPurchaseService = inAppPurchaseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<object>> Handle(GenerateTokenPaymentAppStoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<object>();
            var iss = "e578fe49-b5c1-4f5b-988f-456e0cbba8c8";
            var bid = "com.fsel.lmsapp.uat";
            var kid = "MD59H6MRVY";
            var iat = ConvertToUnixTimestamp(DateTimeOffset.UtcNow);
            var exp = ConvertToUnixTimestamp(DateTimeOffset.UtcNow.AddMinutes(60));
            var aud = "appstoreconnect-v1";

            var header = new Dictionary<string, object>()
            {
                { "alg", "ES256" },
                { "kid", kid },
                { "typ", "JWT" }
            };

            var payload = new
            {
                iss = iss,
                iat = iat,
                exp = exp,
                aud = aud,
                bid = bid
            };

            string privateKey = File.ReadAllText(ResourceSettings.AppStore);

            byte[] privateKeyBytes = Convert.FromBase64String(ExtractBase64FromPem(privateKey));

            CngKey key = CngKey.Import(privateKeyBytes, CngKeyBlobFormat.Pkcs8PrivateBlob);

            string token = JWT.Encode(payload, key, JwsAlgorithm.ES256, header);

            var getToken = await _inAppPurchaseService.GetNotification(token);
            var statusToken = await _inAppPurchaseService.GetStatusNotification(getToken.Content?.TestNotificationToken, token);
            return methodResult;
        }

        private static long ConvertToUnixTimestamp(DateTimeOffset dateTime)
        {
            DateTimeOffset epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            TimeSpan timeDifference = dateTime - epoch;

            return (long)timeDifference.TotalSeconds;
        }

        private static string ExtractBase64FromPem(string pemContent)
        {
            const string beginMarker = "-----BEGIN PRIVATE KEY-----";
            const string endMarker = "-----END PRIVATE KEY-----";

            int startIndex = pemContent.IndexOf(beginMarker);
            int endIndex = pemContent.IndexOf(endMarker);

            if (startIndex < 0 || endIndex < 0)
            {
                throw new InvalidOperationException("Invalid PEM format.");
            }

            startIndex += beginMarker.Length;
            int base64Length = endIndex - startIndex;

            string base64Content = pemContent.Substring(startIndex, base64Length).Trim();

            return base64Content;
        }
    }
}
