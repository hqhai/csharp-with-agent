// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

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

        public SubmitAnswerCommandHandler(IRepository<TestGroupResult> testGroupResult, IServiceProvider serviceProvider)
        {
            _testGroupResult = testGroupResult;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<PtStateModel>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            var flowTestResult = await _testGroupResult.Queryable.Where(x => x.StudentId == request.StudentId && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                 .Include(x => x.TestResults)
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

            var aggregate = new FlowTestResultAggregate(flowTestResult, _serviceProvider);

            await aggregate.MakeAnswers(new SubmitAnswerCommandModel
            {
                SectionResultId = request.SectionResultId,
                TestResultId = request.TestResultId,
                Answers = request.Answers,
                IsSubmit = request.IsSubmit
            });

            return new MethodResult<PtStateModel>
            {
                Result = await aggregate.ExpotStateData()
            };
        }
    }
}
