// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class TestConfig : Entity
    {
        /// <summary>
        /// Tên bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Mã bài test
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public bool IsActive { get; set; }

        // ProgramId
        public Guid? ProgramId { get; set; }

        public virtual Category? Program { get; set; }
        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public ICollection<TestLayout>? TestLayouts { get; set; }
    }
}
