// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using CourseChangeQuery;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckDonePtByStudentIdQuery : IRequest<MethodResult<PtStateModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckDonePtByStudentIdHandler : IRequestHandler<CheckDonePtByStudentIdQuery, MethodResult<PtStateModel>>
    {
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly IMediator _mediator;

        public CheckDonePtByStudentIdHandler(IRepository<TestGroupResult> testGroupResult, IMediator mediator)
        {
            _testGroupResult = testGroupResult;
            _mediator = mediator;
        }

        public async Task<MethodResult<PtStateModel>> Handle(CheckDonePtByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var getUserNavigationResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!getUserNavigationResult.IsOK || getUserNavigationResult.Result == null)
            {
                return new MethodResult<PtStateModel> { StatusCode = StatusCodes.Status400BadRequest, };
            }

            var userNavigation = getUserNavigationResult.Result;
            if (userNavigation.Status == EnumNavigateActionStatus.NotDoingYetAnything
                || userNavigation.Status == EnumNavigateActionStatus.ChooseProgram)
            {
                return new MethodResult<PtStateModel>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Result = new PtStateModel
                    {
                        StudentId = request.StudentId,

                        Status = EnumResultStatus.NotStarted
                    }
                };
            }
            else
            {
                var testGroupResult = await _testGroupResult.ReadQueryable
                 .Include(x => x.CurrentLevel)
                 .OrderByDescending(x => x.CreatedDate)
                 .FirstOrDefaultAsync(x => x.Id == userNavigation.PtResultId, cancellationToken);

                var ptState = new PtStateModel
                {
                    FlowId = testGroupResult?.FlowId,
                    TestGroupResultId = testGroupResult?.Id,
                    StudentId = request.StudentId,
                    Level = testGroupResult?.CurrentLevel?.Name,
                    LevelId = testGroupResult?.CurrentLevel?.Id,
                    SelectedProgramId = testGroupResult?.ProgramId,
                    SelectedPtProgramId = testGroupResult?.ProgramIdOfPt,
                    Status = testGroupResult?.Status ?? EnumResultStatus.NotStarted
                };

                return new MethodResult<PtStateModel> { StatusCode = StatusCodes.Status200OK, Result = ptState };
            }
        }
    }
}
