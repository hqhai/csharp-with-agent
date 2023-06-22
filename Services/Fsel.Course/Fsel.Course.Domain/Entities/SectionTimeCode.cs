// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class SectionTimeCode : Entity
    {
        /// <summary>
        /// Tên SectionTimeCode
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Thời gian bắt đầu xuất hiện TimeCode
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double DisplayTime { get; set; }

        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double ExecutionTime { get; set; }

        [NotMapped]
        public TimeSpan DisplayTimeSpan
        {
            get { return TimeSpan.FromSeconds(DisplayTime); }
        }

        [NotMapped]
        public TimeSpan ExecutionTimeSpan
        {
            get { return TimeSpan.FromSeconds(ExecutionTime); }
        }

        public Section? Section { get; set; }
        public Guid SectionId { get; set; }
    }
}
