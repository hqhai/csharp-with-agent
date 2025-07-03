// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Shared.Enums;

    public class LeadsIntegrationModel : IntegrationModel
    {
        public EnumIntegrationStatus? Status { get; set; }

        public DateTime? StartTrial { get; set; }

        public DateTime? ExpireDate { get; set; }

        public long? AccessTime { get; set; }

        public string? SchoolGrade { get; set; }

        public string? SchoolClass { get; set; }

        public string? HumanCode { get; set; }

        public string? OTPPhoneNumber { get; set; }

        public string? OTPEmail { get; set; }

        public EnumCourseLevel CurrentLevel { get; set; }

        public IList<OrderIntegrationModel>? OrderIntegrations { get; set; }

        public IList<CourseIntegrationModel>? CourseIntegrations { get; set; }

        public IList<CourseSuggestModel>? CourseSuggests { get; set; }
    }
}
