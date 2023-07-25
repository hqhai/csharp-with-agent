// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class UpdateSupportQuestionCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public bool IsFrequent { get; set; }

        public string? Content { get; set; }

        public bool IsActive { get; set; }

        public Guid SupportCategoryId { get; set; }
    }
}
