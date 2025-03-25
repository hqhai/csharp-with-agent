// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Android
{
    using Fsel.Shared.Constants;
    using Google.Apis.AndroidPublisher.v3;
    using Google.Apis.AndroidPublisher.v3.Data;
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;

    public class GooglePlayBillingService : IDisposable, IGooglePlayBillingService
    {
        private readonly AndroidPublisherService _service;
        private bool _disposed;

        public GooglePlayBillingService()
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
        }

        public async Task<SubscriptionPurchase> VerifySubscriptionAsync(string packageName, string subscriptionId, string token)
        {
            var request = _service.Purchases.Subscriptions.Get(packageName, subscriptionId, token);
            return await request.ExecuteAsync();
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
