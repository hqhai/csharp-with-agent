// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class CourseTeacher : Entity
    {
        public Course? Course { get; set; }

        public Guid TeacherId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Quốc tịch
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Nationality { get; set; }

        /// <summary>
        /// Trình độ
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Deggree { get; set; }

        /// <summary>
        /// Kinh nghiệm
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Experience { get; set; }

        /// <summary>
        /// Số lượng từ
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Strength { get; set; }
    }
}
