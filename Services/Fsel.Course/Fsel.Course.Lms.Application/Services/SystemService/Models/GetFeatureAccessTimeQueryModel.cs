// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class GetFeatureAccessTimeQueryModel
    {
        public Guid? CreatedUserId { get; set; }

        public DateTime? LastVisited { get; set; }

        public long AccessTime { get; set; }

        public EnumFeature EnumFeature { get; set; }
    }
}
