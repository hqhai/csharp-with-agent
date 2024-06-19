// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateFrequentStatusSupportQuestionCommandModel : BaseCommandModel
    {
        public bool IsFrequent { get; set; }
    }
}
