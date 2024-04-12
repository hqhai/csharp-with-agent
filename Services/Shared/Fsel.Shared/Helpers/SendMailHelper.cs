// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Threading;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;

    public static class SendMailHelper
    {
        private const string Red = "#C0404C";
        private const string Yellow = "#FFAE46";
        private const string Green = "#53BF65";

        public static string GetColorText(long value1, long value2)
        {
            if (value1 > value2)
            {
                return Green;
            }
            else if (value1 == value2)
            {
                return Yellow;
            }
            else
            {
                return Red;
            }
        }

        public static (string, string, string) ConvertEnum(EnumCourseSkill skill)
        {
            return skill switch
            {
                EnumCourseSkill.Reading => ("rgb(189,134,227)", "Kĩ năng đọc", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillReading_1712116761.png"),
                EnumCourseSkill.Writing => ("rgb(56, 177, 241)", "Kĩ năng viết", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillWriting_1712116810.png"),
                EnumCourseSkill.Speaking => ("rgb(89, 102, 255)", "Kĩ năng nói", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillSpeaking_1712116825.png"),
                EnumCourseSkill.Listening => ("rgb(83, 191, 101)", "Kĩ năng nghe", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillListening_1712116789.png"),
                EnumCourseSkill.Vocabulary => ("rgb(255, 66, 141)", "Từ vựng", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillVocabulary_1712116859.png"),
                _ => ("rgb(255, 174, 70)", "Ngữ pháp", "https://s3-sgn10.fptcloud.com/fsel-public/Images/SkillGrammar_1712116844.png")
            };
        }

        public static string FormatTimeSpanAsClock(long totalMinutes)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);

            int hours = timeSpan.Hours;
            int minutes = timeSpan.Minutes;

            return $"{hours}h{minutes:D2}ph";
        }

        public static async Task<string> GetTemplateFromPath(string baseDirectory, string filePath, CancellationToken cancellationToken)
        {
            var path = Path.Combine(baseDirectory, filePath);
            using StreamReader streamReader = new StreamReader(path);
            var file = await streamReader.ReadToEndAsync(cancellationToken);
            return file;
        }

        public static (string, string) Compare(long value1, long value2)
        {
            if (value1 < value2)
            {
                return (Red, SendMailSetting.Reduced);
            }
            else if (value1 == value2)
            {
                return (Yellow, SendMailSetting.Equal);
            }
            else
            {
                return (Green, SendMailSetting.Increase);
            }
        }
    }
}
