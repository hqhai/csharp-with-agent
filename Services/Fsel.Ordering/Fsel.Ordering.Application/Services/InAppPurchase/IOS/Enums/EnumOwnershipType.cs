// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Enums
{
    using System.Runtime.Serialization;

    public enum EnumOwnershipType
    {
        [EnumMember(Value = "PURCHASED")]
        Purchased = 1,

        [EnumMember(Value = "FAMILY_SHARED")]
        FamilyShared = 2
    }
}
