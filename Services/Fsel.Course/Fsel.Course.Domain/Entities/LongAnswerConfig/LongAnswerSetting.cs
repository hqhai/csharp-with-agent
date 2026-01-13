// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.LongAnswerConfig
{
    using System.ComponentModel.DataAnnotations;
    using Common.Enums.ErrorCodes;
    using Core.Entities;

    public class LongAnswerSetting : Entity
    {
        public Guid ObjectId { get; set; }
        [Range(0, 25, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? Row { get; set; }
        [Range(0, 25, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? Column { get; set; }
    }
}
