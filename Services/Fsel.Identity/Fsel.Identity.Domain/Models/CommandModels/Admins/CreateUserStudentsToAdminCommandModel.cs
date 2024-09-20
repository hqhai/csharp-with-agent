// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using Fsel.Shared.Enums;

    public class CreateUserStudentsToAdminCommandModel
    {
        public IList<string>? Emails { get; set; }
        public bool IsTrialRegistration { get; set; }
        public Guid CourseId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public EnumPaymentRevenueType PaymentRevenueType { get; set; }
    }
}
