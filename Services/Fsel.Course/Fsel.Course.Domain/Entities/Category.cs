// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
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

        public Category? CategoryParent { get; set; }

        public ICollection<Category> Categorys { get; set; } = new List<Category>();

        public ICollection<Level> Levels { get; set; } = new List<Level>();
    }
}
