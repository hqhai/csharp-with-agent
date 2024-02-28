// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchaseService.Models
{
    using Fsel.Ordering.Infrastructure.ValueSettings;

    public class PurchaseValidationSettings
    {
        public PurchaseValidationSettings()
        { }

        public PurchaseValidationSettings(AppStore appStore, GooglePlay googlePlay)
        {
            AppStore = appStore;
            GooglePlay = googlePlay;
        }

        public AppStore? AppStore { get; set; }
        public GooglePlay? GooglePlay { get; set; }
    }

    public class AppStore
    {
        public string? SharedSecret { get; set; }

        public AppStore()
        { }

        public AppStore(AppSetting appSetting)
        {
            SharedSecret = appSetting?.PurchaseValidationSettings?.AppStore?.SharedSecret;
        }
    }

    public class GooglePlay
    {
        public string? BundleId { get; set; }

        public GooglePlay()
        { }

        public GooglePlay(AppSetting appSetting)
        {
            BundleId = appSetting?.PurchaseValidationSettings?.GooglePlay?.BundleId;
        }
    }
}
