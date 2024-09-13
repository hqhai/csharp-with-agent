// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System;
    using global::System.ComponentModel.DataAnnotations;

    public class LuckyTicket : Entity
    {
        public Guid LessonResultId { get; set; }
        public Guid StudentId { get; set; }

        [MaxLength(6, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Ticket { get; set; }

        public DateTime? WinningDate { get; set; }
        public EnumLuckyTicketStatus Status { get; set; }
    }
}
