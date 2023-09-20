// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;

    public class FeatureAccessTimesByUnitIdQueryModel
    {
        public Guid CourseId { get; set; }
        public IList<Guid>? UnitIds { get; set; }
        public Guid UserId { get; set; }
    }
}
