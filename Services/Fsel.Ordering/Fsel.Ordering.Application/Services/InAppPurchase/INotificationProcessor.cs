// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;

    public interface INotificationProcessor
    {
        void Process(AppleNotification notification);
    }
}
