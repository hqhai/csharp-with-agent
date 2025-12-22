// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class MessageHistory : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? From { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? To { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? BCC { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CC { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SMSId { get; set; }

        public EnumMessageHistoryType Type { get; set; }

        public EnumMessageHistoryStatus Status { get; set; }

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        public EnumSenderTemplate? Template { get; set; }

        public Guid? ReceiverId { get; set; }
    }
}
