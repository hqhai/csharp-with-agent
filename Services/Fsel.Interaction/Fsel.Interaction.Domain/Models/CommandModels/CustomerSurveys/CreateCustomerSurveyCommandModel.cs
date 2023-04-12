// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys
{
    public class CreateCustomerSurveyCommandModel
    {
        public string? UserId { get; set; }
        public IList<CreateSurveyCommandModel>? Answers { get; set; }
    }

    public class CreateSurveyCommandModel
    {
        public Guid SurveyQuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
