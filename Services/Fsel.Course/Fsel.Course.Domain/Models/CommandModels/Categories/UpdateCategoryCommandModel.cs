// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Categories
{
    using Fsel.Shared.Enums;

    public class UpdateCategoryCommandModel : CreateCategoryCommandModel
    {
        public Guid Id { get; set; }

        public EnumStatus Status { get; set; }
    }

}
