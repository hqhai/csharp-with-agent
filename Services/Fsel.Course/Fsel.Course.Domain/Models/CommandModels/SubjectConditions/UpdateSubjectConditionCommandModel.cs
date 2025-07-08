// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SubjectConditions
{
    using System;
    using Fsel.Course.Domain.Models.CommandModels.SubjectConditionRules;

    public class UpdateSubjectConditionCommandModel
    {
        public Guid Id { get; set; }

        public bool Status { get; set; }

        public IList<UpdateSubjectConditionRuleCommandModel>? SubjectConditionRules { get; set; }
    }
}
