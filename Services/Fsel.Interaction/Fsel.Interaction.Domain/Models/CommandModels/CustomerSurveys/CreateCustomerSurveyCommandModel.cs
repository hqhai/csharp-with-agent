// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys
{
    using Fsel.Common.Attributes;

    public class CreateCustomerSurveyCommandModel
    {
        public IList<CreateSurveyCommandModel>? Answers { get; set; }

        public Guid? UserId { get; set; }

        [EmailValid]
        public string? Email { get; set; }
    }

    public class CreateSurveyCommandModel
    {
        public Guid Id { get; set; }
        public object? Answer { get; set; }
        public bool IsCompleted { get; set; }

    }
}
