// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class Parent : Entity
    {
        [Required(ErrorMessage = nameof(EnumParentErrorCode.PA01C))]
        [MaxLength(250, ErrorMessage = nameof(EnumParentErrorCode.PA02C))]
        public string? Gender { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumParentErrorCode.PA02C))]
        public string? Occupation { get; set; }

        public Human? Human { get; set; }
        public Guid HumanId { get; set; }
        public List<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
