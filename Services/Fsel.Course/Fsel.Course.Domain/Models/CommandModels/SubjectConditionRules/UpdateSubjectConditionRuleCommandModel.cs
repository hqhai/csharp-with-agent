// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules
{
    public class UpdateSubjectConditionRuleCommandModel : CreateSubjectConditionRuleCommandModel
    {
        public Guid? Id { get; set; }
    }
}
