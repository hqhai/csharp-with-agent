// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class TeachingCost : Entity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// giá của writing
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double WritingCost { get; set; }

        /// <summary>
        /// giá của Speapking
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double SpeapkingCost { get; set; }

        /// <summary>
        /// giá của Live Lesson
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double LiveLessonCost { get; set; }
    }
}
