// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CustomerSurveyModel : BaseCommandModel
    {
        public object? Answer { get; set; }
        public Guid? UserId { get; set; }
        public Guid SurveyQuestionId { get; set; }

        public bool IsCompleted { get; set; }
    }
}
