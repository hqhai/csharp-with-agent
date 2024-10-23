// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeBusinessModel
    {
        public EnumFeatureBussinessType FeatureBusinessType { get; set; }
        public long TotalVisit { get; set; }
        public long AccessTime { get; set; }
    }
}
