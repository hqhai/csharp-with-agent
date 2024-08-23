// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums
{
    using System.Runtime.Serialization;

    public enum EnumPurchaseType
    {
        [EnumMember(Value = "Auto-Renewable Subscription")] AutoRenewSub,

        [EnumMember(Value = "Non-Consumable")] NonConsumable,

        [EnumMember(Value = "Consumable")] Consumable,

        [EnumMember(Value = "Non-Renewing Subscription")] NoRenewSub,
    }
}
