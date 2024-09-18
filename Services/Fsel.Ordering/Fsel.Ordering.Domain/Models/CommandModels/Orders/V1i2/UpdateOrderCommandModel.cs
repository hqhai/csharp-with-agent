// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class UpdateOrderCommandModel
    {
        public Order? Order { get; set; }
        public Package? Package { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Code { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid EventId { get; set; }
        public bool IsInvoice { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? ReferralCode { get; set; }
        public string? VoucherCode { get; set; }
    }
}
