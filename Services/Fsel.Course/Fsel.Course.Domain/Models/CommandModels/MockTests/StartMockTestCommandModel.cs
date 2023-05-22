// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTests
{
    using System;

    public class StartMockTestCommandModel
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid MockTestId { get; set; }
    }
}
