// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeBusinessModel
    {
        public EnumFeatureBussinessType FeatureBusinessType { get; set; }
        public long TotalVisit { get; set; }
        public long AccessTime { get; set; }
    }
}
