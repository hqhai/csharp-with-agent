// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class AddFeatureMissionQueueModel
    {
        public Guid ReceiverId { get; set; }
        public EnumFeatureUserReferral FeatureUserReferral { get; set; }
        public int? Token { get; set; }
        public Guid? PackageId { get; set; }
    }
}
