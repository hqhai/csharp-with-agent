// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class CategoryTreeModel
    {
        public string? Code { get; set; }
        public string? Label { get; set; }
        public Guid Data { get; set; }
        public EnumTypeCategory ExpandedIcon { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public IList<CategoryTreeModel>? Children { get; set; }
    }
}
