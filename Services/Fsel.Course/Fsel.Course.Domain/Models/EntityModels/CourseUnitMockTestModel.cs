// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseUnitMockTestModel
    {
        public int DisplayOrder { get; set; }

        public Guid? UnitId { get; set; }

        public UnitModel? Unit { get; set; }

        public Guid? MockTestId { get; set; }

        public MockTestModel? MockTest { get; set; }
    }
}
