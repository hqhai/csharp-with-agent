// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs
{
    using Fsel.Course.Domain.Enums;

    public class ConditionRule
    {
        public EnumSubjectConditionRuleType Type { get; set; }

        public IList<Guid>? LevelIds { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public EnumOperatorType OperatorType { get; set; }
    }
}
