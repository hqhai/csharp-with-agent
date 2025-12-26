// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.SubjectConditionRuleConfigModels
{
    using Fsel.Course.Domain.Enums;

    public class ConditionValueModel : BaseProgramLevelModel
    {
        public EnumSubjectConditionValueType Type { get; set; }

        public int? Maximum { get; set; }
    }
}
