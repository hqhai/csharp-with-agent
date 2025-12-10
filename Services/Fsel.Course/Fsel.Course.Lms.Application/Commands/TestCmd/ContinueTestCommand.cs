// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices.Aggregates;

    public class ContinueTestCommand : IRequest<MethodResult<SingleTestStateModel>>
    {
        public Guid TestResultId { get; set; }
    }

    public class ContinueTestCommandHandler : IRequestHandler<ContinueTestCommand, MethodResult<SingleTestStateModel>>
    {
        private IRepository<TestResult> _testResult;
        private IRepository<TestGroupResult> _testGroupResult;
        private readonly IServiceProvider _serviceProvider;

        public ContinueTestCommandHandler(IRepository<TestResult> testResult, IServiceProvider serviceProvider, IRepository<TestGroupResult> testGroupResult)
        {
            _testResult = testResult;
            _serviceProvider = serviceProvider;
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<SingleTestStateModel>> Handle(ContinueTestCommand request, CancellationToken cancellationToken)
        {
            var testResult = await _testResult.ReadQueryable.Include(x => x.TestGroupResult)
                .FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);
            if (testResult == null)
            {
                var result = new MethodResult<SingleTestStateModel> { StatusCode = 400, };

                result.AddErrorBadRequest("No active test found for the student.", "ContinueTestCommandHandler");
                return result;
            }

            var testGroupResult = await _testGroupResult.ReadQueryable.FirstOrDefaultAsync(x => x.Id == testResult.TestGroupResultId, cancellationToken);

            var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);
            await aggregate.InitAggregate();

            if (testResult.Status != EnumResultStatus.Done && testResult.Status != EnumResultStatus.ByPass)
            {
                await aggregate.Start();
            }

            return new MethodResult<SingleTestStateModel> { Result = await aggregate.ExpotStateData() };
        }
    }
}
