// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserReferrals
{
    using System;
    using Fsel.Shared.Enums;

    public class AddFeatureMissionCommandModel
    {
        public Guid ReceiverId { get; set; }
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
        public int? Token { get; set; }
        public Guid? PackageId { get; set; }
    }
}
