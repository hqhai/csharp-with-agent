// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Graphs
{
    using Fsel.Course.Domain.Entities.TestConfigs;

    public interface IGraphComponent
    {
        Node? GetNextNode();

        bool AssignStepResult(TestResult testResult);
    }
}
