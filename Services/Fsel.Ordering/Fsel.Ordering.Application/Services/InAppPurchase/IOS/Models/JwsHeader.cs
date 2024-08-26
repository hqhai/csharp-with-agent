// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase.IOS.Models
{
    public class JwsHeader
    {
        public string Alg { get; set; }
        public string[] X5C { get; set; }
    }
}
