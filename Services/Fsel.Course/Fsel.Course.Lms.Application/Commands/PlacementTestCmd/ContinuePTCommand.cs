// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ContinuePTCommand : IRequest<MethodResult<PtStateModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class ContinuePTCommandHandler : IRequestHandler<ContinuePTCommand, MethodResult<PtStateModel>>
    {
        private IRepository<TestGroupResult> _testGroupResult;
        private readonly IServiceProvider _serviceProvider;
        private readonly MediatR.IMediator _mediator;

        public ContinuePTCommandHandler(IRepository<TestGroupResult> testGroupResult, IServiceProvider serviceProvider, MediatR.IMediator mediator)
        {
            _testGroupResult = testGroupResult;
            _serviceProvider = serviceProvider;
            _mediator = mediator;
        }

        public async Task<MethodResult<PtStateModel>> Handle(ContinuePTCommand request, CancellationToken cancellationToken)
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

            var flowTestResult = await _testGroupResult.Queryable
                .Where(x => x.Id == navigateActionResult.Result.PtResultId.Value)
                .Include(x => x.CourseChangingHistories)
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

            var aggregate = new FlowTestResultAggregate(flowTestResult, _serviceProvider);
            await aggregate.InitAggregate();

            if (flowTestResult.Status != Domain.Enums.EnumResultStatus.Done && flowTestResult.Status != Domain.Enums.EnumResultStatus.ByPass)
            {
                await aggregate.Start();
            }

            return new MethodResult<PtStateModel>
            {
                Result = await aggregate.ExpotStateData()
            };
        }
    }
}
