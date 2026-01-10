// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.TestModels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Queues.Publishers.Test;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices.Aggregates;

    public class TestSubmitAnswerCommand : IRequest<MethodResult<SingleTestStateModel>>
    {
        public Guid SectionResultId { get; set; }

        public Guid TestResultId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }

        public bool IsSubmit { get; set; }
    }

    public class TestSubmitAnswerCommandHandler : IRequestHandler<TestSubmitAnswerCommand, MethodResult<SingleTestStateModel>>
    {
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly SubmitAiTestLayOutPublisher _submitAiTestLayOutPublisher;
        private readonly IServiceProvider _serviceProvider;

        public TestSubmitAnswerCommandHandler(ITestResultRepository testResultRepository,
            IServiceProvider serviceProvider,
            ITestGroupResultRepository testGroupResultRepository,
            SubmitAiTestLayOutPublisher submitAiTestLayOutPublisher)
        {
            _testGroupResultRepository = testGroupResultRepository;
            _submitAiTestLayOutPublisher = submitAiTestLayOutPublisher;
            _serviceProvider = serviceProvider;
            _testResultRepository = testResultRepository;
        }

        public async Task<MethodResult<SingleTestStateModel>> Handle(TestSubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SingleTestStateModel>();

            var testResult = await _testResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.TestResultId, cancellationToken);

            if (testResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testResult), request.TestResultId);
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

            var testGroupResult = await _testGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == testResult.TestGroupResultId, cancellationToken);
            if (testGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testGroupResult), testResult.TestGroupResultId);
                return methodResult;
            }
            var aggregate = new TestResultAggregate(testGroupResult, _serviceProvider, testResult);

            await aggregate.MakeTestAnswers(new SubmitAnswerCommandModel
            {
                SectionResultId = request.SectionResultId,
                TestResultId = request.TestResultId,
                Answers = request.Answers,
                IsSubmit = request.IsSubmit,
            });
            await _submitAiTestLayOutPublisher.Publish(new BaseQueueModel { QueueId = request.SectionResultId.ToString() }, cancellationToken);

            methodResult.Result = await aggregate.ExpotStateData();
            return methodResult;
        }
    }
}
