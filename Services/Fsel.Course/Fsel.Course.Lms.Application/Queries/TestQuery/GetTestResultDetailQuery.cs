// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.TestQuery
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Models.EntityModels.PlacementTestModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestStateModel>();

            var testResult = await _testService.LoadHierachicalTestResult(x => x.Id == request.TestResultId);
            if (testResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var testResultComposite = new TestResultComposite { Result = testResult, ServiceProvider = _serviceProvider };
            testResultComposite.GenerateChildren();
            var testResultState = testResultComposite.ExportState();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = testResultState as TestStateModel;

            return methodResult;
        }
    }
}
