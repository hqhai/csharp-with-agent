// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.V1i1
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;

    public class CourseModule : Entity, IDisplayInfo
    {
        public EnumCourseConfigType CourseConfigType { get; set; }

        public int DisplayOrder { get; set; }

        public int DisplayNumber { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }

        public Course? Course { get; set; }

        public Guid CourseId { get; set; }

        public Guid OriginalId { get; set; }

        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
    }
}
