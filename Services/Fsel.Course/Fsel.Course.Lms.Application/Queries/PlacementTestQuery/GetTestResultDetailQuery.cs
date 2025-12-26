// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using Common.ActionResults;
    using Domain.Models.EntityModels.PlacementTestModels;
    using MediatR;
    using Services.ApplicationServices;
    using Services.ApplicationServices.Aggregates;

    public class GetTestResultDetailQuery : IRequest<MethodResult<TestStateModel>>
    {
        public Guid TestResultId { get; set; }
    }

    public class GetTestResultDetailQueryHandler : IRequestHandler<GetTestResultDetailQuery, MethodResult<TestStateModel>>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITestService _testService;

        public GetTestResultDetailQueryHandler(ITestService testService, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _testService = testService;
        }

        public async Task<MethodResult<TestStateModel>> Handle(GetTestResultDetailQuery request, CancellationToken cancellationToken)
        {
            var testResult = await _testService.LoadHierachicalTestResult(x => x.Id == request.TestResultId);
            if (testResult == null)
            {
                return new MethodResult<TestStateModel>();
            }
            var testResultComposite = new TestResultComposite { Result = testResult, ServiceProvider = _serviceProvider };
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTestHierarchicalData();
            var testResultState = testResultComposite.ExportState() as TestStateModel;
            testResultState.UpdateDetailInfo(testResultComposite.Test);
            return new MethodResult<TestStateModel>
            {
                Result = testResultState
            };
        }
    }
}
