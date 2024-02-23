// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.UnitResults
{
    using System;
    public class OpenNextUnitForExtendUserCommandModel
    {

        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }

        public Guid UnitId { get; set; }
    }
}
