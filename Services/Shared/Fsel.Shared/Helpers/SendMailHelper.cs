// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public static class SendMailHelper
    {

        public static string GetColorText(long value1, long value2)
        {
            if (value1 > value2)
            {
                return "#53BF65";
            }
            else if (value1 == value2)
            {
                return "#FFAE46";
            }
            else
            {
                return "#C0404C";
            }
        }

        public static (string, string, string) ConvertEnum(EnumCourseSkill skill)
        {
            if (skill == EnumCourseSkill.Reading)
                return ("rgb(189,134,227)", "Kĩ năng đọc", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillReading_1706698217.png");
            else if (skill == EnumCourseSkill.Writing)
                return ("rgb(56,238,195)", "Kĩ năng viết", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillWriting_1706698232.png");
            else if (skill == EnumCourseSkill.Speaking)
                return ("rgb(88,160,255)", "Kĩ năng nói", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillSpeaking_1706698202.png");
            else if (skill == EnumCourseSkill.Listening)
                return ("rgb(217,234,77)", "Kĩ năng nghe", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillListening_1706698183.png");
            else if (skill == EnumCourseSkill.Vocabulary)
                return ("rgb(255,174,70)", "Từ vựng", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillVocabulary_1706698261.png");
            else
                return ("rgb(255,215,70)", "Ngữ pháp", "https://s3-sgn10.fptcloud.com/fsel/Images/SkillGrammar_1706698247.png");
        }

        public static string FormatTimeSpanAsClock(long totalMinutes)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;

            return $"{hours}h{minutes:D2}ph";

        }
    }
}
