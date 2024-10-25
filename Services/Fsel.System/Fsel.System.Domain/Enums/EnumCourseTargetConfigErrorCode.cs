// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    public enum EnumCourseTargetConfigErrorCode
    {
        TitleNotNull,

        TitleAlreadyExist,

        NonNegativeLessonNumberPerWeek,

        NonNegativeMaxHoursPerLesson,

        CourseTagetAlreadyExist,

        CourseLevelMismatchesCourseType
    }
}
