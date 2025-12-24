// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices.Interface
{
    public interface ITestAiLayoutService
    {
        Task EvaluateAsync(Guid testSectionResultId, CancellationToken cancellationToken);
    }
}
