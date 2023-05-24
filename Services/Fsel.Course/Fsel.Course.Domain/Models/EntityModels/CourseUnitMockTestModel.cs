// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseUnitMockTestModel
    {
        public int DisplayOrder { get; set; }
        public Guid CouseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? FinalTestId { get; set; }
        public Guid? MockTestId { get; set; }
    }
}
