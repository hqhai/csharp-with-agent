// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class UserReferralToken
    {
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
        public int Token { get; set; }
    }
}
