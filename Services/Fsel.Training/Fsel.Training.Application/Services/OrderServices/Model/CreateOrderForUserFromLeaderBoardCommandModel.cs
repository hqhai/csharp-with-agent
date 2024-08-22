// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.OrderServices.Model
{
    using System;
    using Fsel.Shared.Enums;

    public class CreateOrderForUserFromLeaderBoardCommandModel
    {
        public Guid UserId { get; set; }
        public int Month { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public Guid PackageId { get; set; }
        public Guid EventId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? Address { get; set; }
        public bool IsInvoice { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? ReferralCode { get; set; }
    }
}
