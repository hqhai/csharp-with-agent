// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Shared.Enums;

    public class ClientsIntegrationModel : IntegrationModel
    {
        public DateTime? StartCourse { get; set; }

        public DateTime? EndCourse { get; set; }

        public IList<OrderIntegrationModel> OrderIntegration { get; set; } = new List<OrderIntegrationModel>();
    }

    public class OrderIntegrationModel
    {
        public string? OrderCode { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Program { get; set; }

        public string? CourseLevel { get; set; }

        public int CoursePackage { get; set; }

        public EnumPaymentMethodStatus PaymentMethod { get; set; }

        public decimal DiscountPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string? Status { get; set; }

        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}
