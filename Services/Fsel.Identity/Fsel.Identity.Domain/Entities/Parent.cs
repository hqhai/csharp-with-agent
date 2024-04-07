// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Parent : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Occupation { get; set; }

        public virtual User? User { get; set; }
        public Guid UserId { get; set; }
        public ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
