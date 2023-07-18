// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TeachingCost : Entity
    {   /// <summary>
        /// Tên khóa học
        /// </summary>
        public EnumCourseLevel CourseLevel { get; set; }

        /// <summary>
        /// giá của writing
        /// </summary>
        public double WritingCost { get; set; }

        /// <summary>
        /// giá của Speapking
        /// </summary>
        public double SpeapkingCost { get; set; }

        /// <summary>
        /// giá của Live Lesson
        /// </summary>
        public double LiveLessonCost { get; set; }
    }
}
