// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CustomerSurveyModel : BaseCommandModel
    {
        public object? Answer { get; set; }
        public string? UserId { get; set; }
        public Guid SurveyQuestionId { get; set; }
    }
}
