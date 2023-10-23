// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class GameplayTimeConfig : Entity
    {
        [Range(0, 10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int RoundNumber { get; set; }
        public EnumGameVocabPDType GameVocabPDType { get; set; }
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public double Time { get; set; }
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public double Percent { get; set; }
    }
}
