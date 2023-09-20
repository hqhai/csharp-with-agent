// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SurveyQuestionInfoModel : BaseModel
    {
        public string? Question { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public float DisplayOrder { get; set; }
        public int DisplayLevel { get; set; }
        public EnumSurveyQuestion Type { get; set; }
        public object? Answers { get; set; }
        public bool IsPilot { get; set; }
        public object? Answer { get; set; }
    }
}
