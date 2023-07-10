// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentSurveyQuestionModel : BaseModel
    {
        public string? Question { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public EnumSurveyQuestion Type { get; set; }
        public object? Answer { get; set; }
    }
}
