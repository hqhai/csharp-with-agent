// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseUnitMockTestModel
    {
        public int DisplayOrder { get; set; }
        public UnitModel? Unit { get; set; }
        public MockTestModel? MockTest { get; set; }
        public FinalTestModel? FinalTest { get; set; }

        public string? Type
        {
            get
            {
                return FinalTestId.HasValue ? nameof(FinalTest) : MockTestId.HasValue ? nameof(MockTest) : UnitId.HasValue ? nameof(Unit) : null;
            }
        }

        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? FinalTestId { get; set; }
        public Guid? MockTestId { get; set; }
        public bool? IsUsed { get; set; }
    }
}
