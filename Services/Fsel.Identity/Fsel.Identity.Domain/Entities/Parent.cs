// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class Parent : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Occupation { get; set; }

        public Human? Human { get; set; }
        public Guid HumanId { get; set; }
        public List<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
