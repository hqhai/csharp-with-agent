// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SurveyQuestions
{
    using Fsel.Shared.Enums;

    public class CreateSurveyQuestionCommandModel
    {
        public string? Question { get; set; }

        public string? Description { get; set; }

        public string? Title { get; set; }

        public string? Icon { get; set; }

        public float DisplayOrder { get; set; }

        public int DisplayLevel { get; set; }

        public bool? IsRequired { get; set; }

        public EnumSurveyFormType SurveyFormType { get; set; }

        public EnumSurveyQuestion Type { get; set; }

        public object? Answers { get; set; }
    }
}
