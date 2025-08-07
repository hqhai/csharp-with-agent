// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Android
{
    using Fsel.Shared.Constants;
    using Google.Apis.AndroidPublisher.v3;
    using Google.Apis.AndroidPublisher.v3.Data;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Microsoft.Extensions.Logging;

    public class GooglePlayBillingService : IDisposable, IGooglePlayBillingService
    {
        private readonly AndroidPublisherService _service;
        private bool _disposed;
        private readonly ILogger<GooglePlayBillingService> _logger;

        public GooglePlayBillingService(ILogger<GooglePlayBillingService> logger)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(ResourceSettings.AndroidPrivateKey, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(AndroidPublisherService.Scope.Androidpublisher);
            }

            _service = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FSEL- Learning English",
            });
            _logger = logger;
        }

        public async Task<SubscriptionPurchaseV2?> VerifySubscriptionAsync(string packageName, string token)
        {
            try
            {
                var request = _service.Purchases.Subscriptionsv2.Get(packageName, token);
                return await request.ExecuteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<ProductPurchase> VerifyProductAsync(string packageName, string productId, string token)
        {
            var request = _service.Purchases.Products.Get(packageName, productId, token);
            return await request.ExecuteAsync();
        }

        // Implementing IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                    _service.Dispose();
                }
                // Dispose unmanaged resources here
                _disposed = true;
            }
        }

        ~GooglePlayBillingService()
        {
            Dispose(false);
        }
    }
}
