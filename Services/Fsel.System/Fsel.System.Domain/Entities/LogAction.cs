// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;

    public class LogAction : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? FunctionType { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ActionType { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ChangeData { get; set; }

        public string? RecordIds { get; set; }
    }
}
