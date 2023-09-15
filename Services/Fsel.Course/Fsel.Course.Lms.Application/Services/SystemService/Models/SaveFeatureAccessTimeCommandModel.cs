// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class SaveFeatureAccessTimeCommandModel
    {
        public EnumFeature EnumFeature { get; set; }
        public long AccessTime { get; set; }
        public DateTime LastVisited { get; set; } = DateTime.Now;
        public Guid ObjectId { get; set; }
    }
}
