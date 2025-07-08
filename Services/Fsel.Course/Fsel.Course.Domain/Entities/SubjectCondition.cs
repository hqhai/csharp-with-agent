// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class SubjectCondition : Entity
    {
        public EnumConditionType Type { get; set; }

        public bool Status { get; set; }

        public Guid CategoryId { get; set; }

        public Category? Category { get; set; }

        public ICollection<SubjectConditionRule> SubjectConditionRules { get; set; } = new List<SubjectConditionRule>();
    }
}
