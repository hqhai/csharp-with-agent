// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class GetFeatureAccessTimesByTestQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public Guid ObjectId { get; set; }
        public EnumFeature EnumFeature { get; set; }
    }
}
