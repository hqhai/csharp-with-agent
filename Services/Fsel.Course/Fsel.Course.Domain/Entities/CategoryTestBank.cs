// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;

    public class CategoryTestBank : Entity
    {
        public EnumTestType TestType { get; set; }
        public Guid TestId { get; set; }
        public Test? Test { get; set; }
        public Category? Category { get; set; }
        public Guid ProgramId { get; set; }
    }
}
