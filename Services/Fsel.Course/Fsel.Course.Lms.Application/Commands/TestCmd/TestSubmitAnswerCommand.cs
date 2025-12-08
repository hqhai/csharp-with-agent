// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.PlacementTestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices.Aggregates;

    public class TestSubmitAnswerCommand : IRequest<MethodResult<TestStateModel>>
    {
        public Guid SectionResultId { get; set; }

        public Guid TestResultId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }

        public bool IsSubmit { get; set; }
    }

    public class TestSubmitAnswerCommandHandler : IRequestHandler<TestSubmitAnswerCommand, MethodResult<TestStateModel>>
    {
        private IRepository<TestResult> _testResult;
        private readonly IServiceProvider _serviceProvider;

        public TestSubmitAnswerCommandHandler(IRepository<TestResult> testResult, IServiceProvider serviceProvider)
        {
            _testResult = testResult;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<TestStateModel>> Handle(TestSubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<TestStateModel>();

            var testResult = await _testResult.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);

            if (testResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (testResult.Status == EnumResultStatus.Done)
            {
                return new MethodResult<TestStateModel>
                {
                    Result = new TestStateModel
                    {
                        Status = testResult.Status,
                    }
                };
            }

            var aggregate = new TestResultAggregate(testResult, _serviceProvider);

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
