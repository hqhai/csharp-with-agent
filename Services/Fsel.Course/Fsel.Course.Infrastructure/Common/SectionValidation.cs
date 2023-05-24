// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Shared.Enums;

    public static class SectionValidation
    {
        public static bool IsCheckSection(EnumCourseSkill courseSkill, int index, int correctTotal)
        {
            if (courseSkill == EnumCourseSkill.Reading)
            {
                return ((index == 1 || index == 3) && correctTotal == 13) || (index == 2 && correctTotal == 14);
            }
            else if (courseSkill == EnumCourseSkill.Listening)
            {
                return correctTotal == 10;
            }

            return false;
        }
    }
}
