// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Shared.Enums;

    public static class ValidateSection
    {
        public static bool IsCheckSection(EnumCourseSkill courseSkill, int index, int correctTotal)
        {
            if (courseSkill == EnumCourseSkill.Reading)
            {
                if (index == 0 && correctTotal == 13)
                {
                    return true;
                }
                else if (index == 1 && correctTotal == 14)
                {
                    return true;
                }
                else if (index == 2 && correctTotal == 13)
                {
                    return true;
                }
            }
            else if (courseSkill == EnumCourseSkill.Listening)
            {
                if (correctTotal == 10)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
