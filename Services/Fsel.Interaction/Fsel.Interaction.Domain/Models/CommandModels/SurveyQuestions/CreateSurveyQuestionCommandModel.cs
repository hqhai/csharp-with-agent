// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SurveyQuestions
{
    public class CreateSurveyQuestionCommandModel
    {
        public Guid SurveyQuestionId { get; set; }
        public object? Answers { get; set; }
    }
}
