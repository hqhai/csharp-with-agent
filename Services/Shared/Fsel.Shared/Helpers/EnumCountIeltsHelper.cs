// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public static class EnumCountIeltsHelper
    {
        private static IList<KeyValuePair<double, double>> s_keyValueReadings = new List<KeyValuePair<double, double>>
        {
            new KeyValuePair<double, double>(0, 0),
            new KeyValuePair<double, double>(1, 1),
            new KeyValuePair<double, double>(2, 1.5),
            new KeyValuePair<double, double>(3, 2),
            new KeyValuePair<double, double>(4, 2.5),
            new KeyValuePair<double, double>(5, 2.5),
            new KeyValuePair<double, double>(6, 3),
            new KeyValuePair<double, double>(7, 3),
            new KeyValuePair<double, double>(8, 3.5),
            new KeyValuePair<double, double>(9, 3.5),
            new KeyValuePair<double, double>(10, 4),
            new KeyValuePair<double, double>(11, 4),
            new KeyValuePair<double, double>(12, 4),
            new KeyValuePair<double, double>(13, 4.5),
            new KeyValuePair<double, double>(14, 4.5),
            new KeyValuePair<double, double>(15, 5),
            new KeyValuePair<double, double>(16, 5),
            new KeyValuePair<double, double>(17, 5),
            new KeyValuePair<double, double>(18, 5),
            new KeyValuePair<double, double>(19, 5.5),
            new KeyValuePair<double, double>(20, 5.5),
            new KeyValuePair<double, double>(21, 5.5),
            new KeyValuePair<double, double>(22, 5.5),
            new KeyValuePair<double, double>(23, 6),
            new KeyValuePair<double, double>(24, 6),
            new KeyValuePair<double, double>(25, 6),
            new KeyValuePair<double, double>(26, 6),
            new KeyValuePair<double, double>(27, 6.5),
            new KeyValuePair<double, double>(28, 6.5),
            new KeyValuePair<double, double>(29, 6.5),
            new KeyValuePair<double, double>(30, 7),
            new KeyValuePair<double, double>(31, 7),
            new KeyValuePair<double, double>(32, 7),
            new KeyValuePair<double, double>(33, 7.5),
            new KeyValuePair<double, double>(34, 7.5),
            new KeyValuePair<double, double>(35, 8),
            new KeyValuePair<double, double>(36, 8),
            new KeyValuePair<double, double>(37, 8.5),
            new KeyValuePair<double, double>(38, 8.5),
            new KeyValuePair<double, double>(39, 9),
            new KeyValuePair<double, double>(40, 9),
        };

        private static IList<KeyValuePair<double, double>> s_keyValueListenings = new List<KeyValuePair<double, double>>
        {
            new KeyValuePair<double, double>(0, 0),
            new KeyValuePair<double, double>(1, 1),
            new KeyValuePair<double, double>(2, 1.5),
            new KeyValuePair<double, double>(3, 2),
            new KeyValuePair<double, double>(4, 2),
            new KeyValuePair<double, double>(5, 2.5),
            new KeyValuePair<double, double>(6, 2.5),
            new KeyValuePair<double, double>(7, 3),
            new KeyValuePair<double, double>(8, 3),
            new KeyValuePair<double, double>(9, 3.5),
            new KeyValuePair<double, double>(10, 3.5),
            new KeyValuePair<double, double>(11, 4),
            new KeyValuePair<double, double>(12, 4),
            new KeyValuePair<double, double>(13, 4.5),
            new KeyValuePair<double, double>(14, 4.5),
            new KeyValuePair<double, double>(15, 4.5),
            new KeyValuePair<double, double>(16, 5),
            new KeyValuePair<double, double>(17, 5),
            new KeyValuePair<double, double>(18, 5.5),
            new KeyValuePair<double, double>(19, 5.5),
            new KeyValuePair<double, double>(20, 5.5),
            new KeyValuePair<double, double>(21, 5.5),
            new KeyValuePair<double, double>(22, 5.5),
            new KeyValuePair<double, double>(23, 6),
            new KeyValuePair<double, double>(24, 6),
            new KeyValuePair<double, double>(25, 6),
            new KeyValuePair<double, double>(26, 6.5),
            new KeyValuePair<double, double>(27, 6.5),
            new KeyValuePair<double, double>(28, 6.5),
            new KeyValuePair<double, double>(29, 6.5),
            new KeyValuePair<double, double>(30, 7),
            new KeyValuePair<double, double>(31, 7),
            new KeyValuePair<double, double>(32, 7.5),
            new KeyValuePair<double, double>(33, 7.5),
            new KeyValuePair<double, double>(34, 7.5),
            new KeyValuePair<double, double>(35, 8),
            new KeyValuePair<double, double>(36, 8),
            new KeyValuePair<double, double>(37, 8.5),
            new KeyValuePair<double, double>(38, 8.5),
            new KeyValuePair<double, double>(39, 9),
            new KeyValuePair<double, double>(40, 9),
        };

        public static double GetReadingCountIelts(this double number)
        {
            return s_keyValueReadings.FirstOrDefault(x => x.Value == number).Key;
        }

        public static double GetListeningCountIelts(this double number)
        {
            return s_keyValueListenings.FirstOrDefault(x => x.Value == number).Key;
        }

        public static EnumCourseLevel GetLevelInPoint(EnumPlacementTestLevel? enumPlacementTestLevel, double? correctCount = 0)
        {
            EnumCourseLevel courseLevel;
            double roundedNumber = Math.Ceiling(correctCount ?? default);
            if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && roundedNumber >= 3 && roundedNumber <= 3.5)
            {
                courseLevel = EnumCourseLevel.RFE;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && roundedNumber >= 4 && roundedNumber <= 4.5)
            {
                courseLevel = EnumCourseLevel.MS1;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && roundedNumber >= 5 && roundedNumber <= 5.5)
            {
                courseLevel = EnumCourseLevel.MS2;
            }
            else if (enumPlacementTestLevel == EnumPlacementTestLevel.IELTS && roundedNumber >= 6 && roundedNumber <= 6.5)
            {
                courseLevel = EnumCourseLevel.MS3;
            }
            else
            {
                switch (enumPlacementTestLevel)
                {
                    case EnumPlacementTestLevel.A1:
                        courseLevel = EnumCourseLevel.A1;
                        break;

                    case EnumPlacementTestLevel.A2:
                        courseLevel = EnumCourseLevel.A2;
                        break;

                    case EnumPlacementTestLevel.B1:
                        courseLevel = EnumCourseLevel.B1;
                        break;

                    case EnumPlacementTestLevel.B1Plus:
                        courseLevel = EnumCourseLevel.B1Plus;
                        break;

                    case EnumPlacementTestLevel.B2:
                        courseLevel = EnumCourseLevel.B2;
                        break;

                    case EnumPlacementTestLevel.C1:
                        courseLevel = EnumCourseLevel.C1;
                        break;

                    default:
                        courseLevel = EnumCourseLevel.A1;
                        break;
                }
            }

            return courseLevel;
        }
    }
}
