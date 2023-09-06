// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys
{
    public class CreateCustomerSurveyCommandModel
    {
        public IList<CreateSurveyCommandModel>? Answers { get; set; }

        public Guid? UserId { get; set; }

        public bool? IsPilot { get; set; }
    }

    public class CreateSurveyCommandModel
    {
        public Guid Id { get; set; }
        public object? Answer { get; set; }
    }
}
