// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    using Fsel.Shared.Enums;

    public class CourseSuggestModel
    {
        public EnumCourseSuggestType Type { get; set; }

        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }

    public enum EnumCourseSuggestType
    {
        /// <summary>
        /// trình độ dễ hơn
        /// </summary>
        Relaxed,

        /// <summary>
        /// trình độ hiện tại
        /// </summary>
        Balanced,

        /// <summary>
        /// trình độ cao hơn
        /// </summary>
        Challenge
    }
}
