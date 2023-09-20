// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class FeatureAccessTimesByTestQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public IList<Guid>? ObjectIds { get; set; }
        public EnumFeature? EnumFeature { get; set; }
    }
}
