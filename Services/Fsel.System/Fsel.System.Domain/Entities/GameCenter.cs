// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;

    public class GameCenter : Entity
    {
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public ICollection<GameVocabulary> GameVocabularies { get; set; } = new List<GameVocabulary>();
    }
}
