// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class CategoryTreeModel
    {
        public Guid Label { get; set; }

        public string? Data { get; set; }

        public EnumTypeCategory ExpandedIcon { get; set; }

        public IList<CategoryTreeModel>? Children { get; set; }
    }
}
