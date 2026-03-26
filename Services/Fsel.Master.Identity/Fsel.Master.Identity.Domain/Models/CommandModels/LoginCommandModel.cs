// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;

namespace Fsel.Master.Identity.Domain.Models.CommandModels
{
    public class MasterLoginCommandModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [MaxLength(254, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [MinLength(8, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public string? Password { get; set; }
    }
}
