// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.SupportCategorys
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateStatusSupportCategoryCommandModel : BaseCommandModel
    {
        public bool IsActive { get; set; }
    }
}
