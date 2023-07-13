// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Entities;

    public class SurveyQuestionModel : BaseModel
    {
        public string? Question { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public EnumSurveyQuestion Type { get; set; }
        public object? Answers { get; set; }
        public IList<CustomerSurvey>? CustomerSurveys { get; set; }
    }
}
