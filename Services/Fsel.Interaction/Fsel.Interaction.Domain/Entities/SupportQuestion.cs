// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class SupportQuestion : Entity
    {
        /// <summary>
        /// Tên loại câu hỏi
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public bool IsFrequent { get; set; }

        /// <summary>
        /// Tiêu đề
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public bool IsActive { get; set; }

        public Guid SupportCategoryId { get; set; }

        public SupportCategory? SupportCategory { get; set; }

        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    }
}
