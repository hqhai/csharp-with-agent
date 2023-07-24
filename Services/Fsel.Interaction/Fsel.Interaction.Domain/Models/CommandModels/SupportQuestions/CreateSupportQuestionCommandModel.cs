// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportQuestions
{
    using System;

    public class CreateSupportQuestionCommandModel
    {
        public string? Name { get; set; }

        public bool IsFrequent { get; set; }

        public string? Content { get; set; }

        public Guid SupportCategoryId { get; set; }
    }
}
