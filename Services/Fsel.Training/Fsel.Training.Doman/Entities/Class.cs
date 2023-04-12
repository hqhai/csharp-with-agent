// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Doman.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Shared.Enums;
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
        /// End Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public EnumClassType Status { get; set; }
    }
}
