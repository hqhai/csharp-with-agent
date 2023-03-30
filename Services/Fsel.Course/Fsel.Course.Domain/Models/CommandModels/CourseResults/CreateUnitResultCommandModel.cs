// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseResults
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class CreateUnitResultCommandModel
    {
        public double? Percent { get; set; }

        public EnumResultStatus Status { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid StudentId { get; set; }
    }
}
