// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices.Model
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class OrderModel : BaseModel
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }
        public double Price { get; set; }
        public double DiscountPercent { get; set; }
        public double DiscountPrice { get; set; }
        public double TotalPrice { get; set; }
        public Guid? CourseId { get; set; }
        public bool IsTrial { get; set; }
        public DateTime? ExpireDate { get; set; }
        public PackageModel? Package { get; set; }
        public Guid UserId { get; set; }
    }
}
