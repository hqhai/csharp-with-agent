// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class HomeWorkAnswer : BaseAnswer
    {
        public bool IsFirstSubmit { get; set; }
        public HomeWorkQuestion? HomeWorkQuestion { get; set; }

        public HomeWorkResult? HomeWorkResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HomeWorkQuestionId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HomeWorkResultId { get; set; }
    }
}
