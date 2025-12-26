// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using Microsoft.EntityFrameworkCore;

    public class TestAiLayoutService : ITestAiLayoutService
    {
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ISpeakingAITestLayoutHandler _speakingHandler;
        private readonly IWritingAITestLayoutHandler _writingHandler;

        public TestAiLayoutService(
            ITestSectionResultRepository testSectionResultRepository,
            ITestResultRepository testResultRepository,
            ISpeakingAITestLayoutHandler speakingHandler,
            IWritingAITestLayoutHandler writingHandler)
        {
            _testSectionResultRepository = testSectionResultRepository;
            _testResultRepository = testResultRepository;
            _speakingHandler = speakingHandler;
            _writingHandler = writingHandler;
        }

        public async Task EvaluateAsync(Guid testSectionResultId, CancellationToken cancellationToken)
        {
            var testSectionResult = await _testSectionResultRepository.Queryable
                .Include(x => x.SectionResults)
                .Include(x => x.TestSection)
                .FirstOrDefaultAsync(x => x.Id == testSectionResultId, cancellationToken);

            if (testSectionResult == null)
            {
                return;
            }

            var layoutType = testSectionResult.TestSection?.LayoutType;
            if (TestLayoutDispatchHelper.ShouldSkip(layoutType))
            {
                return;
            }

            var testResult = await _testResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == testSectionResult.TestResultId, cancellationToken);
            if (testResult == null)
            {
                return;
            }

            // ✅ Hàm chung check layout + route
            await TestLayoutDispatchHelper.DispatchByLayoutAsync(
                layoutType!.Value,
                testSectionResult,
                testResult,
                speakingHandler: () => _speakingHandler.HandleAsync(testSectionResult, testResult, cancellationToken),
                writingHandler: () => _writingHandler.HandleAsync(testSectionResult, testResult, cancellationToken)
            ).ConfigureAwait(false);
        }
    }
}
