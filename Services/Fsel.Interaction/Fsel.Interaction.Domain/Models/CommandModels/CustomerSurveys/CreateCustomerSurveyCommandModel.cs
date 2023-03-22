// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys
{
    public class CreateCustomerSurveyCommandModel
    {
        public object? Answers { get; set; }
        public Guid UserId { get; set; }
        public Guid SurveyQuestionId { get; set; }
    }
}
