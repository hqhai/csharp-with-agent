// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class CategoryTreeDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public EnumTypeCategory Type { get; set; }
        public IList<CategoryTreeDtoModel> Children { get; set; } = new List<CategoryTreeDtoModel>();
        public IList<LevelTreeDtoModel> Levels { get; set; } = new List<LevelTreeDtoModel>();
    }

    public class LevelTreeDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
