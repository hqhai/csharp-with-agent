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

    public class TargetBandScoreConfig
    {
        public TargetBandScoreConfig(EnumCourseLevel courseLevel, double score)
        {
            CourseLevel = courseLevel;
            Score = score;
        }

        public EnumCourseLevel CourseLevel { get; set; }
        public double Score { get; set; }
    }

    public static class TargetBandScoreHelper
    {
        private static IList<TargetBandScoreConfig> s_targetBandScoreConfigs = new List<TargetBandScoreConfig>
        {
            new TargetBandScoreConfig(EnumCourseLevel.MS1, 5),
            new TargetBandScoreConfig(EnumCourseLevel.MS2, 6),
            new TargetBandScoreConfig(EnumCourseLevel.MS3, 7),
        };

        public static (bool, double) CheckScoreColor(this EnumCourseLevel courseLevel, double score)
        {
            var targetBandScoreConfig = s_targetBandScoreConfigs.FirstOrDefault(x => x.CourseLevel == courseLevel);
            if (targetBandScoreConfig != null)
            {
                return (score >= targetBandScoreConfig.Score, targetBandScoreConfig.Score);
            }
            return (false, default);
        }

        public static double GetBandScore(this EnumCourseLevel courseLevel)
        {
            var targetBandScoreConfig = s_targetBandScoreConfigs.FirstOrDefault(x => x.CourseLevel == courseLevel);
            return targetBandScoreConfig?.Score ?? default;
        }
    }

    public static class IeltsScoreHelper
    {
        public const double MaxScorePT = 6.5;
        private const int ChildrenAge = 13;
        private const int StudentAge = 14;

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
            new IeltsScoreConfig(30, 7, 7),
            new IeltsScoreConfig(31, 7, 7),
            new IeltsScoreConfig(32, 7, 7.5),
            new IeltsScoreConfig(33, 7.5, 7.5),
            new IeltsScoreConfig(34, 7.5, 7.5),
            new IeltsScoreConfig(35, 8, 8),
            new IeltsScoreConfig(36, 8, 8),
            new IeltsScoreConfig(37, 8.5, 8.5),
            new IeltsScoreConfig(38, 8.5, 8.5),
            new IeltsScoreConfig(39, 9, 9),
            new IeltsScoreConfig(40, 9, 9),
        };

        public static double GetIeltsScorePT(this int number, EnumCourseSkill skill)
        {
            double score = number.GetIeltsScore(skill);
            return score >= MaxScorePT ? MaxScorePT : score;
        }

        public static double GetIeltsScore(this int number, EnumCourseSkill skill)
        {
            var config = s_ieltsScoreConfigs.OrderBy(x => x.Number).FirstOrDefault(x => x.Number == number);
            if (config != null)
            {
                return skill == EnumCourseSkill.Listening ? config.ListeningScore : config.ReadingScore;
            }
            return default;
        }

        public static int GetInitialAge(EnumPlacementTestLevel? level, int age = default)
        {
            if (level.HasValue)
            {
                return level == EnumPlacementTestLevel.A2 ? ChildrenAge : StudentAge;
            }
            return age;
        }

        public static (EnumCourseLevel?, bool) GetLevelInScore(this EnumPlacementTestLevel enumPlacementTestLevel, double? value = 0, int? yearOld = 0)
        {
            EnumCourseLevel? courseLevel;
            bool isLock = false;
            if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 4 && value <= 4.5)
            {
                courseLevel = EnumCourseLevel.MS1;
                isLock = true;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 5 && value <= 5.5)
            {
                courseLevel = EnumCourseLevel.MS2;
                isLock = true;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && value >= 6)
            {
                courseLevel = EnumCourseLevel.MS3;
                isLock = true;
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
                        if (yearOld >= 14)
                        {
                            isLock = value >= 75;
                        }
                        courseLevel = value >= 75 ? EnumCourseLevel.B1 : EnumCourseLevel.A1;
                        break;

                    case EnumPlacementTestLevel.B1:
                        if (yearOld <= 13)
                        {
                            isLock = value < 75;
                            courseLevel = !isLock ? EnumCourseLevel.B1Plus : EnumCourseLevel.B1;
                        }
                        else
                        {
                            courseLevel = value >= 75 ? EnumCourseLevel.B1Plus : EnumCourseLevel.A2;
                        }

                        break;

                    case EnumPlacementTestLevel.B1Plus:
                        if (yearOld <= 13)
                        {
                            isLock = true;
                            courseLevel = value >= 75 ? EnumCourseLevel.B2 : EnumCourseLevel.B1Plus;
                        }
                        else
                        {
                            isLock = value < 75;
                            courseLevel = !isLock ? EnumCourseLevel.B2 : EnumCourseLevel.B1Plus;
                        }
                        break;

                    case EnumPlacementTestLevel.B2:
                        isLock = true;
                        if (yearOld <= 13)
                        {
                            courseLevel = value >= 75 ? EnumCourseLevel.B2 : EnumCourseLevel.B1;
                        }
                        else
                        {
                            courseLevel = value >= 75 ? EnumCourseLevel.C1 : EnumCourseLevel.B2;
                        }
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

        public static class PlacementTestHelper
        {
            private const int numberOfPoints = 10;

            public class AssessmentPlacementTestModel
            {
                public EnumCourseLevel DesiredLevel { get; set; }
                public EnumCourseLevel StartingLevel { get; set; }
                public IList<LevelScoreModel> Levels { get; set; } = new List<LevelScoreModel>();
            }

            public class LevelScoreModel
            {
                public EnumPlacementTestLevel Level { get; set; }
                public int CorrectCount { get; set; }
                public int CorrectTotal { get; set; }
            }

            public static (int, int) GetCorrectCountToLevel(EnumCourseLevel desiredLevel, EnumCourseLevel startingLevel, EnumPlacementTestLevel placementTestLevel)
            {
                var assessment = listAssessment.FirstOrDefault(x => x.DesiredLevel == desiredLevel && x.StartingLevel == startingLevel);
                if (assessment != null)
                {
                    var leveScore = assessment.Levels.FirstOrDefault(x => x.Level == placementTestLevel);
                    if (leveScore != null)
                    {
                        return (leveScore.CorrectCount, leveScore.CorrectTotal);
                    }
                }
                return default;
            }

            private static readonly List<AssessmentPlacementTestModel> listAssessment = new List<AssessmentPlacementTestModel>
            {
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFB1,
                   StartingLevel = EnumCourseLevel.A2,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFB1,
                   StartingLevel = EnumCourseLevel.B1,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFA2,
                   StartingLevel = EnumCourseLevel.A2,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFA2,
                   StartingLevel = EnumCourseLevel.B1,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFA1,
                   StartingLevel = EnumCourseLevel.A2,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=0, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.EFA1,
                   StartingLevel = EnumCourseLevel.B1,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=0, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.A1,
                   StartingLevel = EnumCourseLevel.B1,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=0, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel
                {
                   DesiredLevel = EnumCourseLevel.A2,
                   StartingLevel = EnumCourseLevel.B1,
                   Levels = new List<LevelScoreModel>
                   {
                       new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                       new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints}
                   }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B1,
                    StartingLevel = EnumCourseLevel.B1,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B1Plus,
                    StartingLevel = EnumCourseLevel.B1,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1Plus,CorrectCount=0, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B2,
                    StartingLevel = EnumCourseLevel.B1,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1Plus,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B2,CorrectCount=0, CorrectTotal=numberOfPoints},
                    }
                },
                 new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.C1,
                    StartingLevel = EnumCourseLevel.B1,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1Plus,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                    }
                },

                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.A1,
                    StartingLevel = EnumCourseLevel.A2,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=0, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.A2,
                    StartingLevel = EnumCourseLevel.A2,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=0, CorrectTotal=0},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B1,
                    StartingLevel = EnumCourseLevel.A2,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=0, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B1Plus,
                    StartingLevel = EnumCourseLevel.A2,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1Plus,CorrectCount=0, CorrectTotal=numberOfPoints},
                    }
                },
                new AssessmentPlacementTestModel{
                    DesiredLevel = EnumCourseLevel.B2,
                    StartingLevel = EnumCourseLevel.A2,
                    Levels = new List<LevelScoreModel>
                    {
                        new LevelScoreModel{Level = EnumPlacementTestLevel.A2,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                        new LevelScoreModel{Level = EnumPlacementTestLevel.B1Plus,CorrectCount=numberOfPoints, CorrectTotal=numberOfPoints},
                    }
                },
            };
        }
    }
}
