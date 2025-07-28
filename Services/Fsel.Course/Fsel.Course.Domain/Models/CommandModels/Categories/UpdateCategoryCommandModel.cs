// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Categories
{
    public class UpdateCategoryCommandModel : CreateCategoryCommandModel
    {
        public Guid Id { get; set; }
    }

}
