// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public class IeltsScoreConfig
    {
        public IeltsScoreConfig(int number, double readingScore, double listeningScore)
        {
            Number = number;
            ReadingScore = readingScore;
            ListeningScore = listeningScore;
        }

        public int Number { get; set; }
        public double ReadingScore { get; set; }
        public double ListeningScore { get; set; }
    }

    public static class IeltsScoreHelper
    {
        private static IList<IeltsScoreConfig> s_ieltsScoreConfigs = new List<IeltsScoreConfig>
        {
            new IeltsScoreConfig(0, 0, 0),
            new IeltsScoreConfig(1, 1, 1),
            new IeltsScoreConfig(2, 1.5, 1.5),
            new IeltsScoreConfig(3, 2, 2),
            new IeltsScoreConfig(4, 2.5, 2.5),
            new IeltsScoreConfig(5, 2.5, 2.5),
            new IeltsScoreConfig(6, 3, 2.5),
            new IeltsScoreConfig(7, 3, 2.5),
            new IeltsScoreConfig(8, 3.5, 3),
            new IeltsScoreConfig(9, 3.5, 3.5),
            new IeltsScoreConfig(10, 4, 3.5),
            new IeltsScoreConfig(11, 4, 4),
            new IeltsScoreConfig(12, 4, 4),
            new IeltsScoreConfig(13, 4.5, 4.5),
            new IeltsScoreConfig(14, 4.5, 4.5),
            new IeltsScoreConfig(15, 5, 4.5),
            new IeltsScoreConfig(16, 5, 5),
            new IeltsScoreConfig(17, 5, 5),
            new IeltsScoreConfig(18, 5, 5.5),
            new IeltsScoreConfig(19, 5.5, 5.5),
            new IeltsScoreConfig(20, 5.5, 5.5),
            new IeltsScoreConfig(21, 5.5, 5.5),
            new IeltsScoreConfig(22, 5.5, 5.5),
            new IeltsScoreConfig(23, 6, 6),
            new IeltsScoreConfig(24, 6, 6),
            new IeltsScoreConfig(25, 6, 6.5),
            new IeltsScoreConfig(26, 6, 6.5),
            new IeltsScoreConfig(27, 6.5, 6.5),
            new IeltsScoreConfig(28, 6.5, 6.5),
            new IeltsScoreConfig(29, 6.5, 6.5),
            new IeltsScoreConfig(30, 6.5, 6.5),
            new IeltsScoreConfig(31, 6.5, 6.5),
            new IeltsScoreConfig(32, 6.5, 6.5),
            new IeltsScoreConfig(33, 6.5, 6.5),
            new IeltsScoreConfig(34, 6.5, 6.5),
            new IeltsScoreConfig(35, 6.5, 6.5),
            new IeltsScoreConfig(36, 6.5, 6.5),
            new IeltsScoreConfig(37, 6.5, 6.5),
            new IeltsScoreConfig(38, 6.5, 6.5),
            new IeltsScoreConfig(39, 6.5, 6.5),
            new IeltsScoreConfig(40, 6.5, 6.5),
        };

        public static double GetIeltsScore(this double number, EnumCourseSkill skill)
        {
            var config = s_ieltsScoreConfigs.OrderBy(x => x.Number).FirstOrDefault(x => x.Number == number);
            if (config != null)
            {
                return skill == EnumCourseSkill.Listening ? config.ListeningScore : config.ReadingScore;
            }
            return default;
        }

        public static (EnumCourseLevel?, bool) GetLevelInScore(this EnumPlacementTestLevel enumPlacementTestLevel, double? value = 0)
        {
            EnumCourseLevel? courseLevel;
            bool isLock = false;
            if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 3 && value <= 3.5)
            {
                courseLevel = EnumCourseLevel.RFE;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 4 && value <= 4.5)
            {
                courseLevel = EnumCourseLevel.MS1;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 5 && value <= 5.5)
            {
                courseLevel = EnumCourseLevel.MS2;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 6)
            {
                courseLevel = EnumCourseLevel.MS3;
            }
            else
            {
                switch (enumPlacementTestLevel)
                {
                    case EnumPlacementTestLevel.A1:
                        isLock = true;
                        if (value >= 75)
                        {
                            courseLevel = EnumCourseLevel.A2;
                        }
                        else if (value >= 25)
                        {
                            courseLevel = EnumCourseLevel.A1;
                        }
                        else
                        {
                            courseLevel = EnumCourseLevel.A1; /*null;*/
                        }
                        break;

                    case EnumPlacementTestLevel.A2:
                        isLock = value >= 75;
                        courseLevel = isLock ? EnumCourseLevel.B1 : EnumCourseLevel.A1;
                        break;

                    case EnumPlacementTestLevel.B1:
                        courseLevel = value >= 75 ? EnumCourseLevel.B1Plus : EnumCourseLevel.A2;
                        break;

                    case EnumPlacementTestLevel.B1Plus:
                        isLock = value < 75;
                        courseLevel = !isLock ? EnumCourseLevel.B2 : EnumCourseLevel.B1Plus;
                        break;

                    case EnumPlacementTestLevel.B2:
                        isLock = true;
                        courseLevel = value >= 75 ? EnumCourseLevel.C1 : EnumCourseLevel.B2;
                        break;

                    case EnumPlacementTestLevel.C1:
                        courseLevel = EnumCourseLevel.C1;
                        break;

                    default:
                        courseLevel = EnumCourseLevel.A1;
                        break;
                }
            }

            return (courseLevel, isLock);
        }
    }
}
