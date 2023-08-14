// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class SupportCategory : Entity
    {
        /// <summary>
        /// Tiêu đề
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        /// <summary>
        /// Tên loại support
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Icon path
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? IconPath { get; set; }

        public bool IsActive { get; set; }
        public ICollection<SupportQuestion> SupportQuestions { get; set; } = new List<SupportQuestion>();

        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
