// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class UpdateCourseUnitMockTestCommandModel
    {
        public int OrderNumber { get; set; }
        public Guid? UnitId { get; set; }

        public Guid? MockTestId { get; set; }
    }
}
