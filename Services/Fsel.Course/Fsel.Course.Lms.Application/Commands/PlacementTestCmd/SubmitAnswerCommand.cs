// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using IMediator = MediatR.IMediator;

    public class SubmitAnswerCommand : IRequest<MethodResult<PtStateModel>>
    {
        public Guid StudentId { get; set; }

        public Guid SectionResultId { get; set; }

        public Guid TestResultId { get; set; }

        public IList<TestAnswerQuestionModel>? Answers { get; set; }

        public bool IsSubmit { get; set; }
    }

    public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, MethodResult<PtStateModel>>
    {
        private IRepository<TestGroupResult> _testGroupResult;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITestSectionResultRepository _testSectionResultRepository;
        private readonly MediatR.IMediator _mediator;
        private readonly SendMailFinishPTPublisher _sendMailFinishPTPublisher;

        public SubmitAnswerCommandHandler(IRepository<TestGroupResult> testGroupResult,
            ITestSectionResultRepository testSectionResultRepository,
            IServiceProvider serviceProvider,
            IMediator mediator,
            SendMailFinishPTPublisher sendMailFinishPTPublisher)
        {
            _testGroupResult = testGroupResult;
            _serviceProvider = serviceProvider;
            _testSectionResultRepository = testSectionResultRepository;
            _mediator = mediator;
            _sendMailFinishPTPublisher = sendMailFinishPTPublisher;
        }

        public async Task<MethodResult<PtStateModel>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            var navigateActionResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!navigateActionResult.IsOK
                || navigateActionResult.Result?.Status != EnumNavigateActionStatus.ContinuePt
                || navigateActionResult.Result?.PtResultId == null)
            {
                var methodResult = new MethodResult<PtStateModel>
                {
                    StatusCode = 400,
                };
                methodResult.AddErrorBadRequest("Not pt is process");
                return methodResult;
            }

            var flowTestResult = await _testGroupResult.Queryable.Where(x => x.Id == navigateActionResult.Result.PtResultId)
                 .Include(x => x.TestResults)
                 .Include(x => x.CourseChangingHistories)
                 .FirstOrDefaultAsync(cancellationToken);

            if (flowTestResult == null)
            {
                var result = new MethodResult<PtStateModel>
                {
                    StatusCode = 400,
                };

                result.AddErrorBadRequest("No active placement test found for the student.", "ContinuePTCommandHandler");

                return result;
            }

            if (flowTestResult.Status == Domain.Enums.EnumResultStatus.Done)
            {
                return new MethodResult<PtStateModel>
                {
                    Result = new PtStateModel
                    {
                        FlowId = flowTestResult.FlowId,
                        Status = flowTestResult.Status,
                    }
                };
            }

            var testSectionResult = await _testSectionResultRepository.Queryable
                .Where(x => x.Id == request.SectionResultId)
                .FirstOrDefaultAsync(cancellationToken);

            if (testSectionResult == null || testSectionResult.Status == Domain.Enums.EnumResultStatus.Done)
            {
                var result = new MethodResult<PtStateModel>
                {
                    StatusCode = 400,
                };
                result.AddErrorBadRequest("Section result not found or already completed.");
                return result;
            }

            var aggregate = new FlowTestResultAggregate(flowTestResult, _serviceProvider, _sendMailFinishPTPublisher);

            await aggregate.MakeAnswers(new SubmitAnswerCommandModel
            {
                SectionResultId = request.SectionResultId,
                TestResultId = request.TestResultId,
                Answers = request.Answers,
                IsSubmit = request.IsSubmit,
                StudentId = request.StudentId
            });

            return new MethodResult<PtStateModel>
            {
                Result = await aggregate.ExpotStateData()
            };
        }
    }
}
