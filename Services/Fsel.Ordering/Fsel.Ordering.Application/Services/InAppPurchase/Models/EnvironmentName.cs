// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.Models
{
    using System.Runtime.Serialization;

    public enum EnvironmentName
    {
        [EnumMember(Value = "Sandbox")]
        Sandbox = 0,

        [EnumMember(Value = "PROD")]
        PROD = 1
    }
}
