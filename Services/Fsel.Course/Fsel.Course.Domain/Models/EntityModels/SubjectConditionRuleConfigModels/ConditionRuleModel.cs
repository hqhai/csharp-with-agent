// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.SubjectConditionRuleConfigModels
{
    using Fsel.Course.Domain.Enums;

    public class ConditionRuleModel : BaseProgramLevelModel
    {
        public EnumSubjectConditionRuleType Type { get; set; }

        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public EnumOperatorType OperatorType { get; set; }
    }
}
