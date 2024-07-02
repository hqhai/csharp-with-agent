// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.InAppPurchases.Androids
{
    public class VerifyDataFromAndroidAppCommandModel
    {
        public string? PackageName { get; set; }
        public string? SubscriptionId { get; set; }
        public string? Token { get; set; }
    }
}
