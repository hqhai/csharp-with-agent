// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateStatusSupportQuestionCommandModel : BaseCommandModel
    {
        public bool IsActive { get; set; }
    }
}
