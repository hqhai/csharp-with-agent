// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;

    public class GameplayRuleConfig : Entity
    {
        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int StartRoundNumber { get; set; }
        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int EndRoundNumber { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int CurrentUnit { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int CurrentUnitOutside { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int PreviousUnit { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int PreviousUnitOutside { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int NumberQuestionPerGame { get; set; }
    }
}
