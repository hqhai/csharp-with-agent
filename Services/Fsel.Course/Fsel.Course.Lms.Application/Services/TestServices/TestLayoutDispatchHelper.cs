// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public static class TestLayoutDispatchHelper
    {
        public static bool ShouldSkip(EnumTestLayoutType? layoutType)
            => layoutType == null || layoutType == EnumTestLayoutType.Basic;

        private static readonly IReadOnlyDictionary<EnumMockTestAIType, EnumCriteriaAi> MockTestCriteriaMap = new Dictionary<EnumMockTestAIType, EnumCriteriaAi>
        {
            { EnumMockTestAIType.TaskAchievement, EnumCriteriaAi.Ta },
            { EnumMockTestAIType.TaskResponse, EnumCriteriaAi.Tr },
            { EnumMockTestAIType.LexicalResource, EnumCriteriaAi.Lr },
            { EnumMockTestAIType.Coherence, EnumCriteriaAi.Cc },
            { EnumMockTestAIType.GrammaticalRange, EnumCriteriaAi.Gra }
        };

        public static EnumCriteriaAi GetCriteriaAi(EnumMockTestAIType scoreCriteria)
        {
            if (!MockTestCriteriaMap.TryGetValue(scoreCriteria, out var result))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scoreCriteria),
                    scoreCriteria,
                    "Invalid criteria for AI evaluation.");
            }

            return result;
        }

        public static EnumMockTestAIType GetTestAIType(EnumCriteriaAi scoreCriteria)
        {
            var result = MockTestCriteriaMap.FirstOrDefault(x => x.Value == scoreCriteria).Key;
            return result;
        }

        private static readonly IReadOnlyDictionary<EnumTestScoreCriteria, EnumCriteriaAi> CriteriaMap = new Dictionary<EnumTestScoreCriteria, EnumCriteriaAi>
        {
            { EnumTestScoreCriteria.FluencyAndCoherence, EnumCriteriaAi.Fc },
            { EnumTestScoreCriteria.LexicalResource, EnumCriteriaAi.Lr },
            { EnumTestScoreCriteria.GrammaticalRangeAndAccuracy, EnumCriteriaAi.Gra }
        };

        public static EnumCriteriaAi GetCriteriaAi(EnumTestScoreCriteria scoreCriteria)
        {
            if (!CriteriaMap.TryGetValue(scoreCriteria, out var result))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scoreCriteria),
                    scoreCriteria,
                    "Invalid criteria for AI evaluation.");
            }

            return result;
        }

        public static async Task DispatchByLayoutAsync(
            EnumTestLayoutType layoutType,
            TestSectionResult testSectionResult,
            TestResult testResult,
            Func<Task> speakingHandler,
            Func<Task> writingHandler)
        {
            switch (layoutType)
            {
                case EnumTestLayoutType.SpeakingMocktest:
                    await speakingHandler().ConfigureAwait(false);
                    return;

                case EnumTestLayoutType.WritingMocktest:
                    await writingHandler().ConfigureAwait(false);
                    return;

                default:
                    // layout khác: skip
                    return;
            }
        }
    }
}
