// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Shared.Enums;

    public static class TestLayoutDispatchHelper
    {
        public static bool ShouldSkip(EnumTestLayoutType? layoutType)
            => layoutType == null || layoutType == EnumTestLayoutType.Basic;

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
