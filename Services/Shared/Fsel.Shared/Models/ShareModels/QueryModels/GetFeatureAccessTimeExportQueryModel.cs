// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetFeatureAccessTimeExportQueryModel
    {
        public Guid UserId { get; set; }
        public Guid? CourseId { get; set; }
        public EnumFeature? EnumFeature { get; set; }
    }
}
