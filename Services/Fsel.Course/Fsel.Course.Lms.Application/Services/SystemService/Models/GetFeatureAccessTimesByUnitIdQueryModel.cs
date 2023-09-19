// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;

    public class GetFeatureAccessTimesByUnitIdQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid UserId { get; set; }
    }
}
