// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using System;

    public class GetFeatureAccessTimeQueryModel
    {
        public Guid? CreatedUserId { get; set; }

        public DateTime? LastVisited { get; set; }

        public long? AccessTime { get; set; }
    }
}
