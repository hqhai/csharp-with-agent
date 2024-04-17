// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ClientsIntegrationModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public EnumGender? EnumGender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Address { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? ParentEmail { get; set; }
        public EnumGender? ParentGender { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTime? LastDate { get; set; }
        public string? PTLevel { get; set; }
        public IList<OrderIntegrationModel> OrderIntegration { get; set; } = new List<OrderIntegrationModel>();
    }

    public class OrderIntegrationModel
    {
        public string? OrderCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Program { get; set; }
        public string? CourseLever { get; set; }
        public int CoursePackage { get; set; }
        public EnumPaymentMethodStatus PaymentMethod { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
