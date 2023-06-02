// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ExtraPraticeChapter : Entity
    {
        /// <summary>
        /// Tên ExtraPractice
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Description 
        /// </summary>
        public string? Description { get; set; }

        public int PageNumber { get; set; }

        public ExtraPractice? ExtraPractice { get; set; }

        public Guid? ExtraPracticeId { get; set; }
    }
}
