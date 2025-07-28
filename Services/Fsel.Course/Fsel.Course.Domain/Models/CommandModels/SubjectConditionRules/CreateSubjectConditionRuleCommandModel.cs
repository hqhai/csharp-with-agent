// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules
{
    using Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs;

    public class CreateSubjectConditionRuleCommandModel
    {
        public IList<ConditionRule>? ConditionRules { get; set; }

        public IList<ConditionValue>? ConditionValues { get; set; }
    }
}
