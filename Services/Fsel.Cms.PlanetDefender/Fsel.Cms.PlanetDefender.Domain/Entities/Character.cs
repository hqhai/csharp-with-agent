// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using System.ComponentModel.DataAnnotations;

    public class Character : Entity
    {
        public bool IsDefault { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }
        public long Price { get; set; }
        public string? Description { get; set; }
        public ICollection<StudentCharacter> StudentCharacters { get; set; } = new List<StudentCharacter>();
    }
}
