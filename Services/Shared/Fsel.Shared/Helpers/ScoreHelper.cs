// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    public static class ScoreHelper
    {
        public static int CalculatePronunciationScore(double accuracy, double? fluency, double? prosody)
        {
            double score;

            // thiếu fluency
            if (!fluency.HasValue)
            {
                score = (0.65 * accuracy) + (0.35 * (prosody ?? 0));
            }
            // thiếu prosody
            else if (!prosody.HasValue)
            {
                score = (0.60 * accuracy) + (0.40 * (fluency ?? 0));
            }
            // đủ cả 3
            else
            {
                score = (0.50 * accuracy) + (0.30 * fluency.Value) + (0.20 * prosody.Value);
            }

            return ConvertToFivePointScale((int)Math.Round(score));
        }

        public static int ConvertToFivePointScale(int pronunciationScore)
        {
            if (pronunciationScore <= 20)
            {
                return 1;
            }
            else if (pronunciationScore <= 40)
            {
                return 2;
            }
            else if (pronunciationScore <= 60)
            {
                return 3;
            }
            else if (pronunciationScore <= 80)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }
    }
}
