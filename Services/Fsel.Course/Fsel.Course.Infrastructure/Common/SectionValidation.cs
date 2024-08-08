// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Shared.Enums;
    using static Fsel.Shared.Constants.ValueSettings;

    public static class SectionValidation
    {
        public static bool IsCheckSection(EnumCourseSkill courseSkill, int index, int correctTotal)
        {
            if (courseSkill == EnumCourseSkill.Reading)
            {
                return ((index == 1 || index == 3) && correctTotal <= SectionGroupIELST.MinScoreSkillReading)
                    || (index == 2 && correctTotal <= SectionGroupIELST.MaxScoreSkillReading);
            }
            else if (courseSkill == EnumCourseSkill.Listening)
            {
                return correctTotal <= SectionGroupIELST.MaxScoreSkillListening;
            }
            return false;
        }
    }
}
