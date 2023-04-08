// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;

    public class HomeWorkAnswer : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Answer { get; set; }

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        public HomeWorkQuestion? HomeWorkQuestion { get; set; }

        public HomeWorkResult? HomeWorkResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid? HomeWorkQuestionId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid? HomeWorkResultId { get; set; }
    }
}
