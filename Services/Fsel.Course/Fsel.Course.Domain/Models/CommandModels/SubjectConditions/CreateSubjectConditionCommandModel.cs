// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SubjectConditions
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules;

    public class CreateSubjectConditionCommandModel
    {
        public EnumConditionType Type { get; set; }

        public bool Status { get; set; }

        public Guid CategoryId { get; set; }

        public IList<CreateSubjectConditionRuleCommandModel>? SubjectConditionRules { get; set; }
    }
}
