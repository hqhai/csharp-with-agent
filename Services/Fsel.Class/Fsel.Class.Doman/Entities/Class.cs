// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Doman.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Class.Doman.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Class : Entity
    {
        /// <summary>
        /// Id Class
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Name Class
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime TimeStart { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        public DateTime TimeEnd { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public EnumClassType Status { get; set; }

        public Guid StudentId { get; set; }
    }
}
