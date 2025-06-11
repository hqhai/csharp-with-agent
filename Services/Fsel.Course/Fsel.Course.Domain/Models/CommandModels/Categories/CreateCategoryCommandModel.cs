// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Categories
{
    using Fsel.Shared.Enums;

    public class CreateCategoryCommandModel
    {
        public Guid? ParentId { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public EnumStatus Status { get; set; }
    }
}
