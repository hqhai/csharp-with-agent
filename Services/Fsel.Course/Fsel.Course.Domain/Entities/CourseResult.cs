// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class CourseResult : Entity
    {
        /// <summary>
        /// Lưu kết quả của Couse
        /// </summary>
        [MinLength(0, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Result { get; set; }

        /// <summary>
        /// Trạng thái của Couse
        /// </summary>
        public EnumCourseStatus Status { get; set; }

        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }
}
