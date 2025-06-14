// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class Category : Entity
    {
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public EnumTypeCategory Type { get; set; }

        public EnumStatus Status { get; set; }

        public Guid? ParentId { get; set; }

        public bool IsTestDefault { get; set; }

        [RequiredIf(nameof(Type), nameof(EnumTypeCategory.Program), ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public EnumTestMode? TestMode { get; set; }

        public Category? CategoryParent { get; set; }

        public ICollection<Category> Categorys { get; set; } = new List<Category>();
        public ICollection<Level> Levels { get; set; } = new List<Level>();
        public ICollection<CategoryTestBank> CategoryTestBanks { get; set; } = new List<CategoryTestBank>();
        public ICollection<Flow> Flows { get; set; } = new List<Flow>();
    }
}
