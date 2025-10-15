// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumCourseGoalCategory
    {
        [Description("Toàn bộ")]
        All,

        [Description("Mặc định (Không bao gồm các học sinh có trường, lớp)")]
        DefaultExcludeSchoolAndClass,

        [Description("Trường")]
        School
    }
}
