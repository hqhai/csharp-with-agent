// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateSupportCategoryCommandModel : BaseCommandModel
    {
        public string? Title { get; set; }

        public string? Name { get; set; }

        public string? IconPath { get; set; }

        public bool IsActive { get; set; }
    }
}
