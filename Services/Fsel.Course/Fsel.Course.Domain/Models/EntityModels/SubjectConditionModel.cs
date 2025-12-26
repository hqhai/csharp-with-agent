// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class SubjectConditionModel : BaseModel
    {
        public EnumConditionType Type { get; set; }

        public bool Status { get; set; }

        public Guid CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public IList<SubjectConditionRuleModel>? SubjectConditionRules { get; set; }
    }
}
