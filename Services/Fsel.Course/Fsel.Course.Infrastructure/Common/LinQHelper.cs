// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Text.RegularExpressions;
    using Fsel.Course.Domain.Models.EntityModels;

    public static class LinQHelper
    {
        public static int GetHighestStreak(this IList<bool>? data)
        {
            if (data != null && data.Any())
            {
                var highestStreak = data.Aggregate(new { CurrentStreak = 0, MaxStreak = 0 },
                                (acc, value) => new
                                {
                                    CurrentStreak = value ? acc.CurrentStreak + 1 : 0,
                                    MaxStreak = value ? Math.Max(acc.MaxStreak, acc.CurrentStreak + 1) : acc.MaxStreak
                                })
                            .MaxStreak - 1;
                return highestStreak > 0 ? highestStreak : default;
            }
            return default;
        }

        public static int GetHighestStreakV1(this IList<AnswerTimelineModel>? data)
        {
            if (data == null || !data.Any())
                return 0;

            int currentScore = 0;
            int maxScore = 0;

            foreach (var item in data)
            {
                currentScore += item.Score ?? 0;

                if (!item.IsCorrect)
                {
                    maxScore = Math.Max(maxScore, currentScore);
                    currentScore = 0;
                }
            }

            maxScore = Math.Max(maxScore, currentScore);

            return maxScore;
        }

        public static int GetHighestStreak(this IList<bool?>? data)
        {
            if (data != null && data.Any())
            {
                var highestStreak = data.Aggregate(new { CurrentStreak = 0, MaxStreak = 0 },
                                (acc, value) => new
                                {
                                    CurrentStreak = value.HasValue && value.Value ? acc.CurrentStreak + 1 : 0,
                                    MaxStreak = value.HasValue && value.Value ? Math.Max(acc.MaxStreak, acc.CurrentStreak + 1) : acc.MaxStreak
                                })
                            .MaxStreak - 1;
                return highestStreak > 0 ? highestStreak : default;
            }
            return default;
        }

        public static bool IsValidCode(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var regex = new Regex(@"^[^<>]*$");
            return regex.IsMatch(input);
        }
    }
}
