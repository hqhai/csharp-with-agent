// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class FinalTestSection : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid FinalTestId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid SectionGroupId { get; set; }

        public SectionGroup? SectionGroup { get; set; }
        public FinalTest? FinalTest { get; set; }
    }
}
