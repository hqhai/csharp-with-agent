// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities;

    public class CreateCourseUnitMockTestCommandModel
    {
        public int OrderNumber { get; set; }
        public Guid? UnitId { get; set; }

        public Guid? MockTestId { get; set; }
    }
}
