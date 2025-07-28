// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.SubjectConditionRuleConfigModels;

    public class SubjectConditionRuleModel : BaseModel
    {
        public IList<ConditionRuleModel>? ConditionRules { get; set; }

        public IList<ConditionValueModel>? ConditionValues { get; set; }
    }
}
