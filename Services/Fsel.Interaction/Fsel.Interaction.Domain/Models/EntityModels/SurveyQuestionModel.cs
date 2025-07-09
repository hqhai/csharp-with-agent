// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;

    public class SurveyQuestionModel : BaseModel
    {
        public string? Question { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public float DisplayOrder { get; set; }
        public int DisplayLevel { get; set; }
        public bool? IsRequired { get; set; }
        public EnumSurveyQuestion Type { get; set; }
        public object? Answers { get; set; }
        public IList<CustomerSurveyModel>? CustomerSurveys { get; set; }
    }

    public class SurveyQuestionTranslationModel : BaseModel, ITranslationObject
    {
        public string? Language { get; set; }
        public string? Question { get; set; }
        public string? Description { get; set; }
        public object? Answers { get; set; }
        public Guid SurveyQuestionId { get; set; }
        public IList<CustomerSurveyModel>? CustomerSurveys { get; set; }
    }
}
