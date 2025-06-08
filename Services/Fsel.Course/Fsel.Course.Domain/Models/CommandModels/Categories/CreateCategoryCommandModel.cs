// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Categories
{
    public class CreateCategoryCommandModel
    {
        public Guid? ParentId { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }
    }
}
