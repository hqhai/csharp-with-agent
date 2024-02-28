// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchaseService.Models
{
    public class PurchaseDetails
    {
        public string ProductID { get; set; }
        public string VerificationData { get; set; }
        public string Platform { get; set; }

        public PurchaseDetails(string productID, string verificationData, string platform)
        {
            ProductID = productID;
            VerificationData = verificationData;
            Platform = platform;
        }

        public bool IsIOS => Platform == "iOS";
        public bool IsAndroid => Platform == "Android";
        public bool IsUnsupported => Platform == "Unsupported";
    }
}
