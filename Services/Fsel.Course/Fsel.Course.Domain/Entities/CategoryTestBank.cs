// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class CategoryTestBank : Entity
    {
        public EnumTestType TestType { get; set; }
        public Guid TestOriginalId { get; set; }
        public Category? Category { get; set; }
        public Guid ProgramId { get; set; }
    }
}
