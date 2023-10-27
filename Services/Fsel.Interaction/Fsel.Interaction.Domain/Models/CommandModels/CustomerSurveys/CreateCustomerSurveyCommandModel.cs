// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.CustomerSurveys
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;

    public class CreateCustomerSurveyCommandModel
    {
        public IList<CreateSurveyCommandModel>? Answers { get; set; }

        public Guid? UserId { get; set; }

        public bool? IsPilot { get; set; }

        [EmailValid]
        public string? Email { get; set; }
    }

    public class CreateSurveyCommandModel
    {
        public Guid Id { get; set; }
        public object? Answer { get; set; }
    }
}
