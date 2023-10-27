// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SupportCategoryModel : BaseModel
    {
        public string? Title { get; set; }

        public string? Name { get; set; }

        public string? IconPath { get; set; }

        public bool IsActive { get; set; }
        public int NumberOfQuestion { get; set; }
    }
}
