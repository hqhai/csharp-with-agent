// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys
{
    public class CreateSupportCategoryCommandModel
    {
        public string? Title { get; set; }

        public string? Name { get; set; }

        public string? IconPath { get; set; }

        public bool IsActive { get; set; }
    }
}
