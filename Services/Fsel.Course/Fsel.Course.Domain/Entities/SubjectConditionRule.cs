// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs;

    public class SubjectConditionRule : Entity
    {
        public string? ConditionRuleStr { get; set; }

        [NotMapped]
        public IList<ConditionRule>? ConditionRules
        {
            get { return ConvertHelper.Deserialize<IList<ConditionRule>>(ConditionRuleStr); }
            set { ConditionRuleStr = ConvertHelper.Serialize(value); }
        }

        public string? ConditionValueStr { get; set; }

        [NotMapped]
        public IList<ConditionValue>? ConditionValues
        {
            get { return ConvertHelper.Deserialize<IList<ConditionValue>>(ConditionValueStr); }
            set { ConditionValueStr = ConvertHelper.Serialize(value); }
        }

        public Guid SubjectConditionId { get; set; }

        public SubjectCondition? SubjectCondition { get; set; }
    }
}
