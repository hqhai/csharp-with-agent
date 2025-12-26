// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices.Interface
{
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;

    public interface ISpeakingAITestLayoutHandler
    {
        Task HandleAsync(TestSectionResult testSectionResult, TestResult testResult, CancellationToken cancellationToken);
    }
}
