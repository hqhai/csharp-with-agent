// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities.Campus
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class SchoolClass : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        public Guid? TeacherId { get; set; }
        public Guid SchoolId { get; set; }

        public IList<Student> Students { get; set; } = new List<Student>();
    }
}
