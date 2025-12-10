// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices.Aggregates;
    using TestStateModel = Domain.Models.EntityModels.PlacementTestModels.TestStateModel;

    public class TestSubmitAnswerCommand : IRequest<MethodResult<SingleTestStateModel>>
    {
        public Guid SectionResultId { get; set; }

        public Guid TestResultId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }

        public bool IsSubmit { get; set; }
    }

    public class TestSubmitAnswerCommandHandler : IRequestHandler<TestSubmitAnswerCommand, MethodResult<SingleTestStateModel>>
    {
        private IRepository<TestResult> _testResult;
        private IRepository<TestGroupResult> _testGroupResult;
        private readonly IServiceProvider _serviceProvider;

        public TestSubmitAnswerCommandHandler(IRepository<TestResult> testResult, IServiceProvider serviceProvider, IRepository<TestGroupResult> testGroupResult)
        {
            _testResult = testResult;
            _serviceProvider = serviceProvider;
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<SingleTestStateModel>> Handle(TestSubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SingleTestStateModel>();

            var testResult = await _testResult.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);

            if (testResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (testResult.Status == EnumResultStatus.Done)
            {
                return new MethodResult<SingleTestStateModel>
                {
                    Result = new SingleTestStateModel
                    {
                        Status = testResult.Status,
                    }
                };
            }

            var testGroupResult = await _testGroupResult.ReadQueryable.FirstOrDefaultAsync(x => x.Id == testResult.TestGroupResultId, cancellationToken);
            var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);

            await aggregate.MakeAnswers(new SubmitAnswerCommandModel
            {
                SectionResultId = request.SectionResultId,
                TestResultId = request.TestResultId,
                Answers = request.Answers,
                IsSubmit = request.IsSubmit,
            });

            methodResult.Result = await aggregate.ExpotStateData();

            return methodResult;
        }
    }
}
