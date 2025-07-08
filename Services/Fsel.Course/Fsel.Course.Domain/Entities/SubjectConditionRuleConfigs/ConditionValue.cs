// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs
{
    using Fsel.Course.Domain.Enums;

    public class ConditionValue
    {
        public EnumSubjectConditionValueType Type { get; set; }

        public IList<Guid>? LevelIds { get; set; }

        /// <summary>
        /// nếu để null = unlimited
        /// </summary>
        public int? Maximum { get; set; }
    }
}
