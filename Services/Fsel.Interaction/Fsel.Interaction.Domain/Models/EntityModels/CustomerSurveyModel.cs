// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Entities;

    public class CustomerSurveyModel : BaseCommandModel
    {
        public string? Answers { get; set; }

        public Guid UserId { get; set; }
        public Guid SurveyQuestionId { get; set; }
    }
}
