// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests
{
    using System;

    public class CreateCourseUnitMockTestCommandModel
    {
        public int DisplayOrder { get; set; }
        public Guid? UnitId { get; set; }

        public Guid? MockTestId { get; set; }
    }
}
