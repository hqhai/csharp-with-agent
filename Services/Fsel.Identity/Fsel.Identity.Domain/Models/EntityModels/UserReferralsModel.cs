// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UserReferralsModel
    {
        public IList<DashboardUserReferralsModel> DashboardUserReferrals { get; set; } = new List<DashboardUserReferralsModel>();
        public IList<UserReferralModel> UserReferrals { get; set; } = new List<UserReferralModel>();
    }

    public class UserReferralModel
    {
        public Guid ReceiverId { get; set; }
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public IList<UserReferralToken>? FeatureMissions { get; set; }
    }

    public class DashboardUserReferralsModel
    {
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
        public int NumberUser { get; set; }
        public int TotalToken { get; set; }
    }
}
