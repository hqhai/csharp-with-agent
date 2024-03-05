// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
    using System.Runtime.Serialization;

    public enum PurchaseType
    {
        [EnumMember(Value = "Auto-Renewable Subscription")] AutoRenewSub,

        [EnumMember(Value = "Non-Consumable")] NonConsumable,

        [EnumMember(Value = "Consumable")] Consumable,

        [EnumMember(Value = "Non-Renewing Subscription")] NoRenewSub,
    }
}
