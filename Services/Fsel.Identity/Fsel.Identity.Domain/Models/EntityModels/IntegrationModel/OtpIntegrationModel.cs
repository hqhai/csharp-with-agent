// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    public class OtpIntegrationModel
    {
        public Guid UserId { get; set; }

        public string? OTPPhoneNumber { get; set; }

        public string? OTPEmail { get; set; }
    }
}
