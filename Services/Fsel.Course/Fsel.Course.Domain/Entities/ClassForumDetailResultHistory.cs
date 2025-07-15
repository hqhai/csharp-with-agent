// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ClassForumDetailResultHistory : Entity
    {
        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? WordContent { get; set; }

        public Guid ClassForumDetailResultId { get; set; }

        public ClassForumDetailResult? ClassForumDetailResult { get; set; }

        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();
    }
}
